IMPORTANT: Please install from NuGet Manager:

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="6.0.16" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="6.0.16" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.16" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.16">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Proxies" Version="6.0.16" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.16" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.16">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>


[2] DATABASE SETUP: EF CORE


APPROACH 1: if all is well, please enter into NuGet Console : 
update-database

Then seed the tables by uncomment in Program.cs: (run once - can comment after tables seeded)
DataInitializer.SeedDatabase(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>()); //Initial seeding of Database tables 


APPROACH 2: if migration not able to update due to version issues,
Please install compatible versions,

remove-migration      <= to first clear any current mgiration folders
add migration INITIAL <= create new migration
update-database

Then seed the tables as above.


APPROACH 3: (Please avoid where possible)
SIDE: Below SQL are as really backups only.
With T8_ERD.png in wwwroot folder for reference.

CREATE TABLE OrderitemCode
(OrderitemCode_ActivationCode uniqueidentifier NOT NUll ,OrderitemCodeCustomerid int NOT NUll,Orderitemid_FK int, 
PRIMARY KEY(OrderitemCode_ActivationCode,OrderitemCodeCustomerid)
);

CREATE TABLE Order
(OrderId int NOT NUll,OrderPurchasedDateTime Datetime,CustomerId_FK int,
PRIMARY KEY(OrderId));


CREATE TABLE OrderItem(
    Orderitemid INT NOT NULL PRIMARY KEY,
    OrderitemQuantity INT,
    OrderitemProductId INT,
    OrderitemName VARCHAR(30),
    OrderitemDescription VARCHAR(50),
    Orderitemimage VARCHAR(30),
    OrderitemPrice INT,
    OrderitemStauts VARCHAR(15),
    OrderId_FK INT,
)

CREATE TABLE Product(
ProductId INT NOT NULL PRIMARY KEY,
ProductName NVARCHAR(MAX),
ProductDescription NVARCHAR(MAX),
ProductImage NVARCHAR(MAX),
ProductPrice decimal(8,2),
ProductStatus NVARCHAR(MAX)
);

CREATE TABLE Customer(
    CustomerId INT NOT NULL PRIMARY KEY,
    UserName NVARCHAR(MAX),
    PASSWORD NVARCHAR(MAX),
    CustomerName NVARCHAR(MAX),
    CustomerStatus NVARCHAR(MAX),
    CustomerGuestCartId UNIQUEIDENTIFIER
);


CREATE TABLE CartItem(
    ProductId_FK INT,
    CustomerId_FK INT ,
    CartItemQuantity INT,
    PRIMARY KEY(ProductId_FK),
    FOREIGN Key(ProductId_FK) REFERENCES Product(ProductId),
    FOREIGN KEY(CustomerId_FK) REFERENCES Customer(CustomerId)
);


CREATE TABLE Review(
    CustomerId_FK INT NOT NUll,
    ProductId_FK INT NOT NUll,
    ReviewScore INT ,
    PRIMARY KEY(CustomerId_FK,ProductId_FK),
    FOREIGN KEY(CustomerId_FK) REFERENCES Customer(CustomerId),
    FOREIGN KEY(ProductId_FK) REFERENCES Product(ProductId)
);


CREATE TABLE SessionLog(
    Id bigint PRIMARY KEY,
    SessionId NVARCHAR(MAX),
    LogSessionCookieStart DATETIME,
    LogSessionCookieExpiry DATETIME,
    CustomerId_FK int ,
    FOREIGN KEY CustomerId_FK REFERENCES Customer(CustomerId)  
);


CREATE TABLE UserLog(
    Id int PRIMARY KEY,
    Timestamp DATETIME2(7),
    IPAddress NVARCHAR(MAX),
    UserAgent NVARCHAR(MAX),
    Actions NVARCHAR(MAX),
    CustomerId_FK int ,
    FOREIGN KEY(CustomerId_FK) REFERENCES Customer(CustomerId)
);

Insert into Customer
VALUES
(1,'ruth',’ ef5MQ6kZIJoQE4+rJDrI4oorIyCOFsGhEI1pE/zcZ08=’,'Ruth',NULL,NULL),
(2,'vincent',' ZcP3VkGyKSXHN8plexJs1ow55CM0nUMDHPmjuaGM7h8=','vincent',NULL,NULL),
(3,'rainne',' VZNrUL/3NU9v1sFU7MbB95WUxbQVQ80tEClNpXcwl2s=','Rainne',NULL,NULL),
(4,'steven',' x8CEMYtvG+zm90/84epTWWBwNFJy3ugEADdJfH1Mv/4=','Steven',NULL,NULL),
(5,'snow',' p0YiLwnYVgXFLU5jZ4jW/9wnRpi5i4xfMkTAaVhoOmk=','Snow',NULL,NULL),
(6,'annabelle',' hswm1xwIS3/4b5eOrI5bTMTFyxa1CxmrnI53ruqF+kU=','Annabelle',NULL,NULL),
(7,'ethan',' e4vWwKv1PSKIi+r8SIMOEVaQfdTsfm6jHlWg3W3FqWk=','Ethan',NULL,NULL);

Insert into Product
VALUES
(1,'.NET Charts','Brings powerful charting capabilities to your .NET applications.','/images/Chart.png',99.0,'O'),
(2,'.NET PayPal','Integrate your .NET applications with PayPal the easy way!','/images/Paypal_logo.png',69.0,'O'),
(3,'.NET ML','Supercharged .NET Machine Learning libraries','/images/ML_NET.png',299.0,'O'),
(4,'.NET Logger','Powerful numerical methods for your .NET simulations','/images/Numerics.png',199.0,'O'),
(5,'.NET Gamer','Realise your dream Game Application with latest Unity Libraries!','/images/SSFF.png',963.69,'O');

