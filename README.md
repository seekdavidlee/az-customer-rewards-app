# az-customer-rewards-app
This is an example project that demonstrates the EasyAuth feature of hosting your web application on Azure App Service. This means you configure with the built-in authentication and expect the App Service to return the appropriate headers with the necessary identity and authorization.

If you have configured the application correctly, you will be prompted to log in, and see your user claims.

# Local Development
When you first start doing local development, you will need to create an App Registration in AAD that represents your application. Be sure to configure the appropriate ID Token and Redirect url which is potentially https://localhost:44371/signin-oidc. If you refer to the Properties/LaunchSetting.json file, you can find the exact port to use.

# Hosting in Azure App Service
You can use the free tier of Azure App Service. Once it is launched, configure the App Service Authentication with AAD as your provider. Then, download the publish profile so you can publish via Visual Studio. Lastly, go to Configuration and add an app setting with EasyAuth as the Key and true as the Value. This helps the application turn on the EasyAuthMiddleWare to hydrate your claims principal via the header information returned.

# Support
There is NO support available. This is just an example and should be treated as such with no implied warranty. 
