using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace borealis_flowers.api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMaster = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstVisit = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastVisit = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VisitorId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Birthday = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specialization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    EstimatedTime = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specialists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    ImgUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Specialists_Specialization_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DateSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateSchedules_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SpecialistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Resolution = table.Column<string>(type: "TEXT", nullable: true),
                    ResolutionSent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Requests_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServicePrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePrice_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePrice_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timeslots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Time = table.Column<int>(type: "INTEGER", nullable: false),
                    Available = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateScheduleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeslots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timeslots_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Timeslots_DateSchedules_DateScheduleId",
                        column: x => x.DateScheduleId,
                        principalTable: "DateSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryTimeslots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeslotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExternalUserId = table.Column<string>(type: "TEXT", nullable: false),
                    FeedbackRequested = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryTimeslots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryTimeslots_Timeslots_TimeslotId",
                        column: x => x.TimeslotId,
                        principalTable: "Timeslots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Specialization",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("20df743a-bdfa-48c8-8eee-40ce4a3f3bde"), "Nail Specialization", true, "Nail" },
                    { new Guid("7fe3e393-71d2-4385-b775-8617126c6f0f"), "Skincare Specialization", true, "Skincare" },
                    { new Guid("d78d53c1-f24a-4d27-86a4-54adaebb3ae5"), "Makeup Specialization", true, "Makeup" },
                    { new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), "Hair Specialization", true, "Hair" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "EstimatedTime", "Name", "Price", "SpecializationId" },
                values: new object[,]
                {
                    { new Guid("1219e0a7-4cbb-4395-a7b8-d4ce3979a9ac"), 35, "Haircut", 15.0, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") },
                    { new Guid("2944556b-ca71-449c-8b2a-0b493b2d3a78"), 35, "Mens Hair Cut", 19.0, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") },
                    { new Guid("2dfda2c0-3dc6-4937-b52a-fca938ac6a63"), 30, "Eyebrows", 20.0, new Guid("7fe3e393-71d2-4385-b775-8617126c6f0f") },
                    { new Guid("5b385e48-5a16-4575-8290-ee173711840c"), 90, "Coloring", 60.0, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") },
                    { new Guid("73e26494-9a6c-43cd-a5f0-486afced5d61"), 45, "Makeup", 20.0, new Guid("7fe3e393-71d2-4385-b775-8617126c6f0f") },
                    { new Guid("ecf67bc9-70f3-4d38-abd6-2e1bbc457756"), 60, "Ladies Cut", 60.0, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") }
                });

            migrationBuilder.InsertData(
                table: "Specialists",
                columns: new[] { "Id", "Address", "City", "FullName", "ImgUrl", "IsActive", "Latitude", "Longitude", "SpecializationId" },
                values: new object[,]
                {
                    { new Guid("278666b8-3503-47b0-b5f6-7139563dace6"), null, null, "Francisco Gutkowski", "http://lorempixel.com/640/480/abstract", true, null, null, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") },
                    { new Guid("5bd8fd04-9613-4c69-85c9-7347572f6289"), null, null, "Emmet Walsh", "http://lorempixel.com/640/480/sports", true, null, null, new Guid("7fe3e393-71d2-4385-b775-8617126c6f0f") },
                    { new Guid("88639ec4-d834-4788-bce4-05cfce258cce"), null, null, "Odessa Russel", "http://lorempixel.com/640/480/animals", true, null, null, new Guid("d78d53c1-f24a-4d27-86a4-54adaebb3ae5") },
                    { new Guid("b23a6e06-ce61-4445-be74-0cfc5f0a0729"), null, null, "Waino Rath", "http://lorempixel.com/640/480/nature", true, null, null, new Guid("20df743a-bdfa-48c8-8eee-40ce4a3f3bde") },
                    { new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), null, null, "Cecile Hahn", "https://loremflickr.com/958/958", true, null, null, new Guid("ff034503-2dad-402a-a7c0-7aa7f2b2d99b") }
                });

            migrationBuilder.InsertData(
                table: "DateSchedules",
                columns: new[] { "Id", "Date", "IsAvailable", "IsWorkingDay", "SpecialistId" },
                values: new object[,]
                {
                    { new Guid("20f92d7b-adec-49c3-88b0-374f45f3e728"), new DateTime(2023, 7, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), false, true, new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7") },
                    { new Guid("249b8b38-2697-45ba-b59d-839f07af4f51"), new DateTime(2023, 7, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), false, true, new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7") },
                    { new Guid("e039320c-8ed6-4838-bf13-6b05b9bdcb09"), new DateTime(2023, 7, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), false, true, new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7") },
                    { new Guid("f91ab0ad-fc6d-4079-a542-92f2be259262"), new DateTime(2023, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), false, true, new Guid("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateSchedules_SpecialistId",
                table: "DateSchedules",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryTimeslots_TimeslotId",
                table: "HistoryTimeslots",
                column: "TimeslotId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CustomerId",
                table: "Requests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SpecialistId",
                table: "Requests",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePrice_ServiceId",
                table: "ServicePrice",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePrice_SpecialistId",
                table: "ServicePrice",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_SpecializationId",
                table: "Services",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialists_SpecializationId",
                table: "Specialists",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_CustomerId",
                table: "Timeslots",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeslots_DateScheduleId",
                table: "Timeslots",
                column: "DateScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryTimeslots");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "ServicePrice");

            migrationBuilder.DropTable(
                name: "Timeslots");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DateSchedules");

            migrationBuilder.DropTable(
                name: "Specialists");

            migrationBuilder.DropTable(
                name: "Specialization");
        }
    }
}
