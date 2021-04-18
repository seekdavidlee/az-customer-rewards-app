using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContosoAuthApp
{
	public static class EasyAuthConstants
	{
		public const string EasyAuthType = "http://schemas.contoso.com/identity/easyauth";
		public const string EasyAuthValue = "true";
		public const string PrincipalName = "X-MS-CLIENT-PRINCIPAL-NAME";
		public const string Principal = "X-MS-CLIENT-PRINCIPAL";
		public const string AgentRoleName = "CS.Agent";
		public const string SupervisorRoleName = "CS.Supervisor";
	}
	// Ref: https://github.com/MaximRouiller/MaximeRouiller.Azure.AppService.EasyAuth

	public class UserClaim
	{
		[JsonProperty("typ")]
		public string Type { get; set; }
		[JsonProperty("val")]
		public string Value { get; set; }
	}

	public class MsClientPrincipal
	{
		[JsonProperty("auth_typ")]
		public string AuthenticationType { get; set; }
		[JsonProperty("claims")]
		public IEnumerable<UserClaim> Claims { get; set; }
		[JsonProperty("name_typ")]
		public string NameType { get; set; }
		[JsonProperty("role_typ")]
		public string RoleType { get; set; }
	}

	// Ref: https://cmatskas.com/working-with-easyauth-app-service-authentication-and-net-core-3-1/

	public class EasyAuthMiddleware
	{
		private readonly RequestDelegate _next;

		public EasyAuthMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context.Request.Headers.ContainsKey(EasyAuthConstants.PrincipalName))
			{
				var azureAppServicePrincipalIdHeader = context.Request.Headers[EasyAuthConstants.PrincipalName].First();

				string msClientPrincipalEncoded = context.Request.Headers[EasyAuthConstants.Principal].First();

				byte[] decodedBytes = Convert.FromBase64String(msClientPrincipalEncoded);
				string msClientPrincipalDecoded = System.Text.Encoding.Default.GetString(decodedBytes);
				MsClientPrincipal clientPrincipal = JsonConvert.DeserializeObject<MsClientPrincipal>(msClientPrincipalDecoded);

				ClaimsPrincipal principal = new ClaimsPrincipal();
				var claims = clientPrincipal.Claims.Select(x =>
				{
					// Microsoft Identity uses the schema
					// http://schemas.microsoft.com/ws/2008/06/identity/claims/role

					var type = (x.Type == "roles") ? "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" : x.Type;
					return new Claim(type, x.Value);

				}).ToList();
				claims.Add(new Claim(EasyAuthConstants.EasyAuthType, EasyAuthConstants.EasyAuthValue));

				principal.AddIdentity(new ClaimsIdentity(claims, clientPrincipal.AuthenticationType, clientPrincipal.NameType, clientPrincipal.RoleType));

				context.User = principal;
			}
			else
			{
				throw new ApplicationException("Unable to authorize!");
			}

			await _next(context);
		}
	}

	public static class EasyAuthMiddlewareExtensions
	{
		public static IApplicationBuilder UseEasyAuthAuthentication(
			this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<EasyAuthMiddleware>();
		}

		public static string DisplayName(this ClaimsPrincipal user)
		{
			return user.FindFirst(x => x.Type == "name").Value;
		}
	}
}
