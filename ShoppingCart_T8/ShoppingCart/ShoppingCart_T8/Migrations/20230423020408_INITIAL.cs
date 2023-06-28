using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCart_T8.Migrations
{
    public partial class INITIAL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_Customer",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerGuestCartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Customer", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Product",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductPrice = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    ProductStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Product", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Order",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderPurchasedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Order", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_TBL_Order_TBL_Customer_CustomerId_FK",
                        column: x => x.CustomerId_FK,
                        principalTable: "TBL_Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_SessionLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogSessionCookieStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogSessionCookieExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_SessionLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_SessionLog_TBL_Customer_CustomerId_FK",
                        column: x => x.CustomerId_FK,
                        principalTable: "TBL_Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_UserLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_UserLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBL_UserLog_TBL_Customer_CustomerId_FK",
                        column: x => x.CustomerId_FK,
                        principalTable: "TBL_Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_CartItem",
                columns: table => new
                {
                    ProductId_FK = table.Column<int>(type: "int", nullable: false),
                    CustomerId_FK = table.Column<int>(type: "int", nullable: false),
                    CartItemQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_CartItem", x => new { x.ProductId_FK, x.CustomerId_FK });
                    table.ForeignKey(
                        name: "FK_TBL_CartItem_TBL_Customer_CustomerId_FK",
                        column: x => x.CustomerId_FK,
                        principalTable: "TBL_Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_CartItem_TBL_Product_ProductId_FK",
                        column: x => x.ProductId_FK,
                        principalTable: "TBL_Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_Review",
                columns: table => new
                {
                    CustomerId_FK = table.Column<int>(type: "int", nullable: false),
                    ProductId_FK = table.Column<int>(type: "int", nullable: false),
                    ReviewScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Review", x => new { x.ProductId_FK, x.CustomerId_FK });
                    table.ForeignKey(
                        name: "FK_TBL_Review_TBL_Customer_CustomerId_FK",
                        column: x => x.CustomerId_FK,
                        principalTable: "TBL_Customer",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBL_Review_TBL_Product_ProductId_FK",
                        column: x => x.ProductId_FK,
                        principalTable: "TBL_Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_OrderItem",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderItemProductId = table.Column<int>(type: "int", nullable: false),
                    OrderItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderItemImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderItemPrice = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    OrderItemtStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderId_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_OrderItem", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_TBL_OrderItem_TBL_Order_OrderId_FK",
                        column: x => x.OrderId_FK,
                        principalTable: "TBL_Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TBL_OrderItemCode",
                columns: table => new
                {
                    OrderItem_ActivationCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemCodeCustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId_FK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_OrderItemCode", x => new { x.OrderItemCodeCustomerId, x.OrderItem_ActivationCode });
                    table.ForeignKey(
                        name: "FK_TBL_OrderItemCode_TBL_OrderItem_OrderItemId_FK",
                        column: x => x.OrderItemId_FK,
                        principalTable: "TBL_OrderItem",
                        principalColumn: "OrderItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_CartItem_CustomerId_FK",
                table: "TBL_CartItem",
                column: "CustomerId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Order_CustomerId_FK",
                table: "TBL_Order",
                column: "CustomerId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrderItem_OrderId_FK",
                table: "TBL_OrderItem",
                column: "OrderId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_OrderItemCode_OrderItemId_FK",
                table: "TBL_OrderItemCode",
                column: "OrderItemId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_Review_CustomerId_FK",
                table: "TBL_Review",
                column: "CustomerId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_SessionLog_CustomerId_FK",
                table: "TBL_SessionLog",
                column: "CustomerId_FK");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_UserLog_CustomerId_FK",
                table: "TBL_UserLog",
                column: "CustomerId_FK");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_CartItem");

            migrationBuilder.DropTable(
                name: "TBL_OrderItemCode");

            migrationBuilder.DropTable(
                name: "TBL_Review");

            migrationBuilder.DropTable(
                name: "TBL_SessionLog");

            migrationBuilder.DropTable(
                name: "TBL_UserLog");

            migrationBuilder.DropTable(
                name: "TBL_OrderItem");

            migrationBuilder.DropTable(
                name: "TBL_Product");

            migrationBuilder.DropTable(
                name: "TBL_Order");

            migrationBuilder.DropTable(
                name: "TBL_Customer");
        }
    }
}
