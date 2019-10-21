## Description
This Web API web app based on .NET Core 2.2 Framework, aim to download xml, word and zip file.

## Functions
+ Authentication: Json web token 
+ Download Files: xml, word, zip
+ Throttle: depends on cache, fault tolerant
+ Distribute cahce: fault tolerant
+ Access DB data: Oracle pooling
+ swagger api: /swagger

### Application Perspective
1. Browser user authenticate with credentials and get an jwt,
2. When Server side receives the request with token,
+ it queries file name from redis cache, if exists, return filename 
+ if not, query data from Oracle DB and generate file in server, then return file name.

## Technical Structure
### Authntication
axios -- body: credenticals-- > authenticate server
authenticate server -- token --> axios

### Download File
axios -- authentication: token -- > API server
API server -- Throttle middleware with cache -->
        -- Validate credenticals with DB -->
        -- Check file name exists in cache(if exists, response filename) -->
        -- Generate physical files in server with DB -->
        -- Response with file name --> axios
axios -- javascript download file via url --> file

### API Parameters
1. Authentication
+ Use Jwt to authenticate user. 
+ Throttle middle defense DDos attack.
```
POST /vkshare/authenticate HTTP/1.1
Content-Type: application/json;charset=UTF-8
{ 
   "UserName":"UserName",
   "Password":"Password",
   "TenantCode":"TenantCode"
}
```
2. Download File

```
GET /vkshare/repair?Owner=test&Plant=test&TagNumber=rvtag&ValveType=RV&FileType=XML&SerialNumber= HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJWS0NBcHAiOiJCMWMreVNKcjNmM0hkUWhadmdiZmpKVE5wQnhkajF5ZGdvazlkY3pqNkVBPSIsIm5iZiI6MTU3MTI5OTM3OSwiZXhwIjoxNTcxMzAyOTc5LCJpYXQiOjE1NzEyOTkzNzl9.pl6hq_DmBSJ7QM_CwNgTU_9CqaxB71OKdMxwkB4_1gI
```


## Note
### Definitions and Abbreviations
+ jwt: [JSON Web Token](https://jwt.io/)
+ axios: [Promise based HTTP client for the browser and node.js](https://github.com/axios/axios)
+ redis: [redis documentation](https://redis.io/documentation)
+ middleware: [Write custom ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-2.2)
+ Dependency Injection Lifetime: [transient, scoped, singleton](https://stackoverflow.com/questions/38138100/addtransient-addscoped-and-addsingleton-services-differences)
+ Swagger: [swagger.io](https://github.com/swagger-api/swagger.io)


### Operating Environment
Windows OS + IIS + Oracle 12c
### Assumptions and Dependencies
Chrome 76 + .Net Core 2.x