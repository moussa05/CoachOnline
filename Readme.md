# Coachs online 



## Interfaces
`IAdmin` - Methods of administrator
`ICoachService` - All user stuff (like adding videos, edits etc.)
`IPaymentService` - All stuff we need to correctly work with Stripe. Now its only about user verification
`IWebhook` - Webhooks from services (currently only from Stripe)

## Structure
``` 
wwwroot/images
```
Images like user avatar

```
wwwroot/uploads
```
Files managed by tus.io, so mostly videos.
```
Controllers
```
API Controllers

```
Email templates
```
:)

```
Implementation
```
Should be named "Services". There are all implementations like Admin, or Data (f.e. account management etc.)

```
Implementation/Exceptions
```
Every stuff with exceptions.
```
Interfaces
```

```
ITSAuth
```
There are interfaces we are using in projects. Its depraced now, because we created new interfaces for authentication.

```
Model
```
Data structure
``` 
Statics
```

Time converters etc. All static Classes.

```
Workers
```
Classes doing stuff mostly in background like clearing old data. 

## Info
 - We are using TUS.io as big file uploader. 
 - In config are stored only development keys
 - At production we are using PostgreSQL
 - In Startup you can find long and ugly stuff starting with `app.UseTus(httpContext => ` its because we have implemented it with their docs. Im not sure if its the best way to do that, propably not.
 - When you run project, Swagger should run at `*:5050` port. 
 - We are **not** using classic rest structure. In this project we are working mostly on GRPC-based structure, so mostly methods you can see are **POST** Requests, and also **AuthToken** is sending in request body *(tus.io is exception in here, because it was mandatory to use it in header without changing body.)*
 - DataContext is configured mostly in Model with Data Adnotations like [Key], so its not mandatory to use OnConfiguring in most of cases :) 