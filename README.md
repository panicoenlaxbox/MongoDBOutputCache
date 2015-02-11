MongoDBOutputCache
==================

Proveedor personalizado de cache para ASP.NET MVC

Espera encontrar los siguientes valores en AppSettings:

    <add key="MongoDBOutputCacheProviderConnectionString" value="mongodb://localhost" />
    <add key="MongoDBOutputCacheProviderDatabaseName" value="aspnet" />
    <add key="MongoDBOutputCacheProviderCollectionName" value="outputCache" />

Es necesario llamar al siguiente código en Application_OnStart
<pre><code>OutputCacheAttribute.ChildActionCache = new MongoDBChildActionCache("My cache");</code></pre>