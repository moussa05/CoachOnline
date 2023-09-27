using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    public partial class afterswitchingtopostgresondev : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
        //protected override void Up(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.CreateTable(
        //        name: "Admins",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Email = table.Column<string>(type: "text", nullable: true),
        //            Password = table.Column<string>(type: "text", nullable: true),
        //            LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Admins", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "BillingPlans",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Name = table.Column<string>(type: "text", nullable: true),
        //            StripeProductId = table.Column<string>(type: "text", nullable: true),
        //            Description = table.Column<string>(type: "text", nullable: true),
        //            StripePriceId = table.Column<string>(type: "text", nullable: true),
        //            Currency = table.Column<string>(type: "text", nullable: true),
        //            IsActive = table.Column<bool>(type: "boolean", nullable: false),
        //            AmountPerMonth = table.Column<decimal>(type: "numeric", nullable: true),
        //            BillingOption = table.Column<byte>(type: "smallint", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_BillingPlans", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "CompanyInfo",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Name = table.Column<string>(type: "text", nullable: true),
        //            City = table.Column<string>(type: "text", nullable: true),
        //            SiretNumber = table.Column<string>(type: "text", nullable: true),
        //            BankAccountNumber = table.Column<string>(type: "text", nullable: true),
        //            RegisterAddress = table.Column<string>(type: "text", nullable: true),
        //            Country = table.Column<string>(type: "text", nullable: true),
        //            VatNumber = table.Column<string>(type: "text", nullable: true),
        //            ZipCode = table.Column<string>(type: "text", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_CompanyInfo", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "MonthlyBalances",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Month = table.Column<int>(type: "integer", nullable: false),
        //            Year = table.Column<int>(type: "integer", nullable: false),
        //            BalanceFull = table.Column<long>(type: "bigint", nullable: false),
        //            BalancaeForWithdrawals = table.Column<decimal>(type: "numeric", nullable: false),
        //            Currency = table.Column<string>(type: "text", nullable: true),
        //            CalculationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_MonthlyBalances", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "Terms",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Url = table.Column<string>(type: "text", nullable: true),
        //            Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Terms", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "UserEpisodeAttachemntPermissions",
        //        columns: table => new
        //        {
        //            UserId = table.Column<int>(type: "integer", nullable: false),
        //            MediaId = table.Column<int>(type: "integer", nullable: false),
        //            CurrentToken = table.Column<string>(type: "text", nullable: true),
        //            CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_UserEpisodeAttachemntPermissions", x => new { x.UserId, x.MediaId });
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "UserWatchedEpisodes",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            UserId = table.Column<int>(type: "integer", nullable: false),
        //            EpisodeId = table.Column<int>(type: "integer", nullable: false),
        //            Day = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            EpisodeWatchedTime = table.Column<decimal>(type: "numeric", nullable: false),
        //            EpisodeDuration = table.Column<decimal>(type: "numeric", nullable: false),
        //            IsWatched = table.Column<bool>(type: "boolean", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_UserWatchedEpisodes", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "AdminLogin",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            AuthToken = table.Column<string>(type: "text", nullable: true),
        //            LoggedIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            AdminId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_AdminLogin", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_AdminLogin_Admins_AdminId",
        //                column: x => x.AdminId,
        //                principalTable: "Admins",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "SubscriptionPrice",
        //        columns: table => new
        //        {
        //            BillingPlanId = table.Column<int>(type: "integer", nullable: false),
        //            StripePriceId = table.Column<string>(type: "text", nullable: true),
        //            Currency = table.Column<string>(type: "text", nullable: true),
        //            Reccuring = table.Column<bool>(type: "boolean", nullable: false),
        //            Amount = table.Column<decimal>(type: "numeric", nullable: true),
        //            Period = table.Column<int>(type: "integer", nullable: true),
        //            PeriodType = table.Column<string>(type: "text", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_SubscriptionPrice", x => x.BillingPlanId);
        //            table.ForeignKey(
        //                name: "FK_SubscriptionPrice_BillingPlans_BillingPlanId",
        //                column: x => x.BillingPlanId,
        //                principalTable: "BillingPlans",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "users",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            EmailAddress = table.Column<string>(type: "text", nullable: true),
        //            FirstName = table.Column<string>(type: "text", nullable: true),
        //            Surname = table.Column<string>(type: "text", nullable: true),
        //            YearOfBirth = table.Column<int>(type: "integer", nullable: false),
        //            City = table.Column<string>(type: "text", nullable: true),
        //            Country = table.Column<string>(type: "text", nullable: true),
        //            PostalCode = table.Column<string>(type: "text", nullable: true),
        //            Adress = table.Column<string>(type: "text", nullable: true),
        //            Gender = table.Column<string>(type: "text", nullable: true),
        //            Password = table.Column<string>(type: "text", nullable: true),
        //            Bio = table.Column<string>(type: "text", nullable: true),
        //            Status = table.Column<byte>(type: "smallint", nullable: false),
        //            UserRole = table.Column<byte>(type: "smallint", nullable: false),
        //            companyInfoId = table.Column<int>(type: "integer", nullable: true),
        //            TermsAcceptedId = table.Column<int>(type: "integer", nullable: true),
        //            StripeAccountId = table.Column<string>(type: "text", nullable: true),
        //            PaymentsEnabled = table.Column<bool>(type: "boolean", nullable: false),
        //            WithdrawalsEnabled = table.Column<bool>(type: "boolean", nullable: false),
        //            StripeCustomerId = table.Column<string>(type: "text", nullable: true),
        //            SubscriptionActive = table.Column<bool>(type: "boolean", nullable: false),
        //            AvatarUrl = table.Column<string>(type: "text", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_users", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_users_CompanyInfo_companyInfoId",
        //                column: x => x.companyInfoId,
        //                principalTable: "CompanyInfo",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //            table.ForeignKey(
        //                name: "FK_users_Terms_TermsAcceptedId",
        //                column: x => x.TermsAcceptedId,
        //                principalTable: "Terms",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "CoachMonthlyBalance",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            CoachId = table.Column<int>(type: "integer", nullable: false),
        //            Month = table.Column<int>(type: "integer", nullable: false),
        //            Year = table.Column<int>(type: "integer", nullable: false),
        //            MonthlyBalanceId = table.Column<int>(type: "integer", nullable: false),
        //            TotalMonthBalance = table.Column<decimal>(type: "numeric", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_CoachMonthlyBalance", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_CoachMonthlyBalance_MonthlyBalances_MonthlyBalanceId",
        //                column: x => x.MonthlyBalanceId,
        //                principalTable: "MonthlyBalances",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_CoachMonthlyBalance_users_CoachId",
        //                column: x => x.CoachId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "courseCategories",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Name = table.Column<string>(type: "text", nullable: true),
        //            AdultOnly = table.Column<bool>(type: "boolean", nullable: false),
        //            ParentId = table.Column<int>(type: "integer", nullable: true),
        //            UserId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_courseCategories", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_courseCategories_courseCategories_ParentId",
        //                column: x => x.ParentId,
        //                principalTable: "courseCategories",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //            table.ForeignKey(
        //                name: "FK_courseCategories_users_UserId",
        //                column: x => x.UserId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "twoFATokens",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Deactivated = table.Column<bool>(type: "boolean", nullable: false),
        //            Token = table.Column<string>(type: "text", nullable: true),
        //            ValidateTo = table.Column<long>(type: "bigint", nullable: false),
        //            Type = table.Column<int>(type: "integer", nullable: false),
        //            UserId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_twoFATokens", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_twoFATokens_users_UserId",
        //                column: x => x.UserId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "UserBillingPlans",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            UserId = table.Column<int>(type: "integer", nullable: false),
        //            BillingPlanTypeId = table.Column<int>(type: "integer", nullable: false),
        //            CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            PlannedActivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
        //            ActivationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
        //            ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
        //            StripeSubscriptionId = table.Column<string>(type: "text", nullable: true),
        //            StripeSubscriptionScheduleId = table.Column<string>(type: "text", nullable: true),
        //            StripePriceId = table.Column<string>(type: "text", nullable: true),
        //            StripeProductId = table.Column<string>(type: "text", nullable: true),
        //            Status = table.Column<byte>(type: "smallint", nullable: false),
        //            IsStudent = table.Column<bool>(type: "boolean", nullable: false),
        //            StudentCardVerificationStatus = table.Column<byte>(type: "smallint", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_UserBillingPlans", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_UserBillingPlans_BillingPlans_BillingPlanTypeId",
        //                column: x => x.BillingPlanTypeId,
        //                principalTable: "BillingPlans",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_UserBillingPlans_users_UserId",
        //                column: x => x.UserId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "userLogins",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            AuthToken = table.Column<string>(type: "text", nullable: true),
        //            Created = table.Column<long>(type: "bigint", nullable: false),
        //            DeviceInfo = table.Column<string>(type: "text", nullable: true),
        //            IpAddress = table.Column<string>(type: "text", nullable: true),
        //            PlaceInfo = table.Column<string>(type: "text", nullable: true),
        //            ValidTo = table.Column<long>(type: "bigint", nullable: false),
        //            Disposed = table.Column<bool>(type: "boolean", nullable: false),
        //            UserId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_userLogins", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_userLogins_users_UserId",
        //                column: x => x.UserId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "CoachDailyBalance",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            CoachBalanceMonthId = table.Column<int>(type: "integer", nullable: false),
        //            BalanceDay = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            BalanceValue = table.Column<decimal>(type: "numeric", nullable: false),
        //            Transferred = table.Column<bool>(type: "boolean", nullable: false),
        //            Calculated = table.Column<bool>(type: "boolean", nullable: false),
        //            TotalEpisodesWatchTime = table.Column<decimal>(type: "numeric", nullable: false),
        //            TransferDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_CoachDailyBalance", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_CoachDailyBalance_CoachMonthlyBalance_CoachBalanceMonthId",
        //                column: x => x.CoachBalanceMonthId,
        //                principalTable: "CoachMonthlyBalance",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "courses",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Name = table.Column<string>(type: "text", nullable: true),
        //            State = table.Column<byte>(type: "smallint", nullable: false),
        //            CategoryId = table.Column<int>(type: "integer", nullable: false),
        //            Description = table.Column<string>(type: "text", nullable: true),
        //            PhotoUrl = table.Column<string>(type: "text", nullable: true),
        //            Created = table.Column<long>(type: "bigint", nullable: false),
        //            UserId = table.Column<int>(type: "integer", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_courses", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_courses_courseCategories_CategoryId",
        //                column: x => x.CategoryId,
        //                principalTable: "courseCategories",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_courses_users_UserId",
        //                column: x => x.UserId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "StudentCardRejection",
        //        columns: table => new
        //        {
        //            SubscriptionId = table.Column<int>(type: "integer", nullable: false),
        //            Reason = table.Column<string>(type: "text", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_StudentCardRejection", x => x.SubscriptionId);
        //            table.ForeignKey(
        //                name: "FK_StudentCardRejection_UserBillingPlans_SubscriptionId",
        //                column: x => x.SubscriptionId,
        //                principalTable: "UserBillingPlans",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "UserStudentCard",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            StudentsCardPhotoName = table.Column<string>(type: "text", nullable: true),
        //            UserBillingPlanId = table.Column<int>(type: "integer", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_UserStudentCard", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_UserStudentCard_UserBillingPlans_UserBillingPlanId",
        //                column: x => x.UserBillingPlanId,
        //                principalTable: "UserBillingPlans",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "Episodes",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            CourseId = table.Column<int>(type: "integer", nullable: false),
        //            Title = table.Column<string>(type: "text", nullable: true),
        //            Description = table.Column<string>(type: "text", nullable: true),
        //            MediaId = table.Column<string>(type: "text", nullable: true),
        //            Created = table.Column<long>(type: "bigint", nullable: false),
        //            OrdinalNumber = table.Column<int>(type: "integer", nullable: false),
        //            MediaNeedsConverting = table.Column<bool>(type: "boolean", nullable: false),
        //            MediaLenght = table.Column<double>(type: "double precision", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Episodes", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_Episodes_courses_CourseId",
        //                column: x => x.CourseId,
        //                principalTable: "courses",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "Rejection",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Reason = table.Column<string>(type: "text", nullable: true),
        //            Date = table.Column<long>(type: "bigint", nullable: false),
        //            CourseId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Rejection", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_Rejection_courses_CourseId",
        //                column: x => x.CourseId,
        //                principalTable: "courses",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "StudentOpenedCourses",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            StudentId = table.Column<int>(type: "integer", nullable: false),
        //            CourseId = table.Column<int>(type: "integer", nullable: false),
        //            FirstOpenedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            LastOpenedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_StudentOpenedCourses", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedCourses_courses_CourseId",
        //                column: x => x.CourseId,
        //                principalTable: "courses",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedCourses_users_StudentId",
        //                column: x => x.StudentId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "EpisodeAttachment",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            Hash = table.Column<string>(type: "text", nullable: true),
        //            Name = table.Column<string>(type: "text", nullable: true),
        //            Extension = table.Column<string>(type: "text", nullable: true),
        //            Added = table.Column<long>(type: "bigint", nullable: false),
        //            EpisodeId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_EpisodeAttachment", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_EpisodeAttachment_Episodes_EpisodeId",
        //                column: x => x.EpisodeId,
        //                principalTable: "Episodes",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "StudentOpenedEpisodes",
        //        columns: table => new
        //        {
        //            Id = table.Column<int>(type: "integer", nullable: false)
        //                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
        //            EpisodeId = table.Column<int>(type: "integer", nullable: false),
        //            CourseId = table.Column<int>(type: "integer", nullable: false),
        //            StudentId = table.Column<int>(type: "integer", nullable: false),
        //            StoppedAtTimestamp = table.Column<decimal>(type: "numeric", nullable: false),
        //            LastWatchDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            FirstOpenDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
        //            WatchedStatus = table.Column<byte>(type: "smallint", nullable: false),
        //            StudentCourseId = table.Column<int>(type: "integer", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_StudentOpenedEpisodes", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedEpisodes_courses_CourseId",
        //                column: x => x.CourseId,
        //                principalTable: "courses",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedEpisodes_Episodes_EpisodeId",
        //                column: x => x.EpisodeId,
        //                principalTable: "Episodes",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedEpisodes_StudentOpenedCourses_StudentCourseId",
        //                column: x => x.StudentCourseId,
        //                principalTable: "StudentOpenedCourses",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //            table.ForeignKey(
        //                name: "FK_StudentOpenedEpisodes_users_StudentId",
        //                column: x => x.StudentId,
        //                principalTable: "users",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //        });

        //    migrationBuilder.CreateIndex(
        //        name: "IX_AdminLogin_AdminId",
        //        table: "AdminLogin",
        //        column: "AdminId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_CoachDailyBalance_CoachBalanceMonthId",
        //        table: "CoachDailyBalance",
        //        column: "CoachBalanceMonthId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_CoachMonthlyBalance_CoachId",
        //        table: "CoachMonthlyBalance",
        //        column: "CoachId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_CoachMonthlyBalance_MonthlyBalanceId",
        //        table: "CoachMonthlyBalance",
        //        column: "MonthlyBalanceId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_courseCategories_ParentId",
        //        table: "courseCategories",
        //        column: "ParentId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_courseCategories_UserId",
        //        table: "courseCategories",
        //        column: "UserId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_courses_CategoryId",
        //        table: "courses",
        //        column: "CategoryId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_courses_UserId",
        //        table: "courses",
        //        column: "UserId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_EpisodeAttachment_EpisodeId",
        //        table: "EpisodeAttachment",
        //        column: "EpisodeId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Episodes_CourseId",
        //        table: "Episodes",
        //        column: "CourseId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_MonthlyBalances_Month_Year",
        //        table: "MonthlyBalances",
        //        columns: new[] { "Month", "Year" });

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Rejection_CourseId",
        //        table: "Rejection",
        //        column: "CourseId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedCourses_CourseId",
        //        table: "StudentOpenedCourses",
        //        column: "CourseId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedCourses_StudentId",
        //        table: "StudentOpenedCourses",
        //        column: "StudentId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedEpisodes_CourseId",
        //        table: "StudentOpenedEpisodes",
        //        column: "CourseId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedEpisodes_EpisodeId",
        //        table: "StudentOpenedEpisodes",
        //        column: "EpisodeId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedEpisodes_StudentCourseId",
        //        table: "StudentOpenedEpisodes",
        //        column: "StudentCourseId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_StudentOpenedEpisodes_StudentId",
        //        table: "StudentOpenedEpisodes",
        //        column: "StudentId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_twoFATokens_UserId",
        //        table: "twoFATokens",
        //        column: "UserId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_UserBillingPlans_BillingPlanTypeId",
        //        table: "UserBillingPlans",
        //        column: "BillingPlanTypeId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_UserBillingPlans_UserId",
        //        table: "UserBillingPlans",
        //        column: "UserId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_userLogins_UserId",
        //        table: "userLogins",
        //        column: "UserId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_users_companyInfoId",
        //        table: "users",
        //        column: "companyInfoId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_users_TermsAcceptedId",
        //        table: "users",
        //        column: "TermsAcceptedId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_UserStudentCard_UserBillingPlanId",
        //        table: "UserStudentCard",
        //        column: "UserBillingPlanId");
        //}

        //protected override void Down(MigrationBuilder migrationBuilder)
        //{
        //    migrationBuilder.DropTable(
        //        name: "AdminLogin");

        //    migrationBuilder.DropTable(
        //        name: "CoachDailyBalance");

        //    migrationBuilder.DropTable(
        //        name: "EpisodeAttachment");

        //    migrationBuilder.DropTable(
        //        name: "Rejection");

        //    migrationBuilder.DropTable(
        //        name: "StudentCardRejection");

        //    migrationBuilder.DropTable(
        //        name: "StudentOpenedEpisodes");

        //    migrationBuilder.DropTable(
        //        name: "SubscriptionPrice");

        //    migrationBuilder.DropTable(
        //        name: "twoFATokens");

        //    migrationBuilder.DropTable(
        //        name: "UserEpisodeAttachemntPermissions");

        //    migrationBuilder.DropTable(
        //        name: "userLogins");

        //    migrationBuilder.DropTable(
        //        name: "UserStudentCard");

        //    migrationBuilder.DropTable(
        //        name: "UserWatchedEpisodes");

        //    migrationBuilder.DropTable(
        //        name: "Admins");

        //    migrationBuilder.DropTable(
        //        name: "CoachMonthlyBalance");

        //    migrationBuilder.DropTable(
        //        name: "Episodes");

        //    migrationBuilder.DropTable(
        //        name: "StudentOpenedCourses");

        //    migrationBuilder.DropTable(
        //        name: "UserBillingPlans");

        //    migrationBuilder.DropTable(
        //        name: "MonthlyBalances");

        //    migrationBuilder.DropTable(
        //        name: "courses");

        //    migrationBuilder.DropTable(
        //        name: "BillingPlans");

        //    migrationBuilder.DropTable(
        //        name: "courseCategories");

        //    migrationBuilder.DropTable(
        //        name: "users");

        //    migrationBuilder.DropTable(
        //        name: "CompanyInfo");

        //    migrationBuilder.DropTable(
        //        name: "Terms");
        //}
    }
}
