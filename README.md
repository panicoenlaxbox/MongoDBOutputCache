MongoDBOutputCache
==================

Proveedor personalizado de cache para ASP.NET MVC

Espera encontrar los siguientes valores en AppSettings:

    <add key="MongoDBOutputCacheProviderConnectionString" value="mongodb://yourhost/yourdatabase" />
    <add key="MongoDBOutputCacheProviderCollection" value="yourcollection" />
    <add key="MongoDBChildActionCacheConnectionString" value="mongodb://yourhost/yourdatabase" />
    <add key="MongoDBChildActionCacheCollection" value="yourcollection" />

Es necesario llamar al siguiente código en Application_OnStart
<pre><code>OutputCacheAttribute.ChildActionCache = new MongoDBChildActionCache("My cache");</code></pre>
