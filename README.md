# SuikaiLauncher.Core Resource Mock API

这里提供了 JRE/JDK、.NET Runtime 等资源，以及核心库的更新信息的静态 API。

>[!WARNING]
>
> Resource Mock API 目前正在施工，最终响应的格式可能会有所不同，请以实际响应为准
>>
> 目前暂时没有资源可供使用，所以大部分内容为空文本或者空列表

## 如何调用

目前暂时只提供 GitHub Raw 的调用方式。

未来可能会提供 GitHub Page 等镜像源。

## JRE/JDK/.NET Runtime

```http
GET https://raw.githubusercontent.com/SuikaiProject/SuikaiLauncher.Core/refs/heads/update/resources.json
```

## SuikaiLauncher.Core Update API

```http
GET https://raw.githubusercontent.com/SuikaiProject/SuikaiLauncher.Core/refs/heads/update/update.json
```

## SuikaiLauncher.Core Version API

```http
GET https://raw.githubusercontent.com/SuikaiProject/SuikaiLauncher.Core/refs/heads/update/versions.json
```
