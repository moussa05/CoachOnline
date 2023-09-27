﻿// <auto-generated />
using System;
using CoachOnline.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CoachOnline.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210627145929_flagged_videos")]
    partial class flagged_videos
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseSerialColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("CoachOnline.Model.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastLogin")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("CoachOnline.Model.AdminLogin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int?>("AdminId")
                        .HasColumnType("integer");

                    b.Property<string>("AuthToken")
                        .HasColumnType("text");

                    b.Property<DateTime>("LoggedIn")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.ToTable("AdminLogin");
                });

            modelBuilder.Entity("CoachOnline.Model.BillingPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<decimal?>("AmountPerMonth")
                        .HasColumnType("numeric");

                    b.Property<byte>("BillingOption")
                        .HasColumnType("smallint");

                    b.Property<string>("Currency")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("StripePriceId")
                        .HasColumnType("text");

                    b.Property<string>("StripeProductId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BillingPlans");
                });

            modelBuilder.Entity("CoachOnline.Model.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<bool>("AdultOnly")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.HasIndex("UserId");

                    b.ToTable("courseCategories");
                });

            modelBuilder.Entity("CoachOnline.Model.CoachBalanceDay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<DateTime>("BalanceDay")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("BalanceValue")
                        .HasColumnType("numeric");

                    b.Property<bool>("Calculated")
                        .HasColumnType("boolean");

                    b.Property<int>("CoachBalanceMonthId")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalEpisodesWatchTime")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("TransferDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Transferred")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("CoachBalanceMonthId");

                    b.ToTable("CoachDailyBalance");
                });

            modelBuilder.Entity("CoachOnline.Model.CoachBalanceMonth", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CoachId")
                        .HasColumnType("integer");

                    b.Property<int>("Month")
                        .HasColumnType("integer");

                    b.Property<int>("MonthlyBalanceId")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalMonthBalance")
                        .HasColumnType("numeric");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("MonthlyBalanceId");

                    b.ToTable("CoachMonthlyBalance");
                });

            modelBuilder.Entity("CoachOnline.Model.CompanyInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<string>("BankAccountNumber")
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("RegisterAddress")
                        .HasColumnType("text");

                    b.Property<string>("SiretNumber")
                        .HasColumnType("text");

                    b.Property<string>("VatNumber")
                        .HasColumnType("text");

                    b.Property<string>("ZipCode")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CompanyInfo");
                });

            modelBuilder.Entity("CoachOnline.Model.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<long>("Created")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("text");

                    b.Property<byte>("State")
                        .HasColumnType("smallint");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("courses");
                });

            modelBuilder.Entity("CoachOnline.Model.Episode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.Property<long>("Created")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("MediaId")
                        .HasColumnType("text");

                    b.Property<double>("MediaLenght")
                        .HasColumnType("double precision");

                    b.Property<bool>("MediaNeedsConverting")
                        .HasColumnType("boolean");

                    b.Property<int>("OrdinalNumber")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("Episodes");
                });

            modelBuilder.Entity("CoachOnline.Model.EpisodeAttachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<long>("Added")
                        .HasColumnType("bigint");

                    b.Property<int?>("EpisodeId")
                        .HasColumnType("integer");

                    b.Property<string>("Extension")
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EpisodeId");

                    b.ToTable("EpisodeAttachment");
                });

            modelBuilder.Entity("CoachOnline.Model.FlaggedCourse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("OrderNo")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("FlaggedCourses");
                });

            modelBuilder.Entity("CoachOnline.Model.MonthlyBalance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<decimal>("BalancaeForWithdrawals")
                        .HasColumnType("numeric");

                    b.Property<long>("BalanceFull")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CalculationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Currency")
                        .HasColumnType("text");

                    b.Property<int>("Month")
                        .HasColumnType("integer");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Month", "Year");

                    b.ToTable("MonthlyBalances");
                });

            modelBuilder.Entity("CoachOnline.Model.Rejection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int?>("CourseId")
                        .HasColumnType("integer");

                    b.Property<long>("Date")
                        .HasColumnType("bigint");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("Rejection");
                });

            modelBuilder.Entity("CoachOnline.Model.Student.UserEpisodeAttachemntPermission", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("MediaId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrentToken")
                        .HasColumnType("text");

                    b.HasKey("UserId", "MediaId");

                    b.ToTable("UserEpisodeAttachemntPermissions");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentCardRejection", b =>
                {
                    b.Property<int>("SubscriptionId")
                        .HasColumnType("integer");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.HasKey("SubscriptionId");

                    b.ToTable("StudentCardRejection");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentCourse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("FirstOpenedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastOpenedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentOpenedCourses");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentEpisode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.Property<int>("EpisodeId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("FirstOpenDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastWatchDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("StoppedAtTimestamp")
                        .HasColumnType("numeric");

                    b.Property<int?>("StudentCourseId")
                        .HasColumnType("integer");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<byte>("WatchedStatus")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("EpisodeId");

                    b.HasIndex("StudentCourseId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentOpenedEpisodes");
                });

            modelBuilder.Entity("CoachOnline.Model.SubscriptionPrice", b =>
                {
                    b.Property<int>("BillingPlanId")
                        .HasColumnType("integer");

                    b.Property<decimal?>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Currency")
                        .HasColumnType("text");

                    b.Property<int?>("Period")
                        .HasColumnType("integer");

                    b.Property<string>("PeriodType")
                        .HasColumnType("text");

                    b.Property<bool>("Reccuring")
                        .HasColumnType("boolean");

                    b.Property<string>("StripePriceId")
                        .HasColumnType("text");

                    b.HasKey("BillingPlanId");

                    b.ToTable("SubscriptionPrice");
                });

            modelBuilder.Entity("CoachOnline.Model.Terms", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Terms");
                });

            modelBuilder.Entity("CoachOnline.Model.TwoFATokens", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<bool>("Deactivated")
                        .HasColumnType("boolean");

                    b.Property<string>("Token")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<long>("ValidateTo")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("twoFATokens");
                });

            modelBuilder.Entity("CoachOnline.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<string>("Adress")
                        .HasColumnType("text");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("text");

                    b.Property<string>("Bio")
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<string>("EmailAddress")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .HasColumnType("text");

                    b.Property<string>("Gender")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<bool>("PaymentsEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("PostalCode")
                        .HasColumnType("text");

                    b.Property<byte>("Status")
                        .HasColumnType("smallint");

                    b.Property<string>("StripeAccountId")
                        .HasColumnType("text");

                    b.Property<string>("StripeCustomerId")
                        .HasColumnType("text");

                    b.Property<bool>("SubscriptionActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Surname")
                        .HasColumnType("text");

                    b.Property<int?>("TermsAcceptedId")
                        .HasColumnType("integer");

                    b.Property<byte>("UserRole")
                        .HasColumnType("smallint");

                    b.Property<bool>("WithdrawalsEnabled")
                        .HasColumnType("boolean");

                    b.Property<int>("YearOfBirth")
                        .HasColumnType("integer");

                    b.Property<int?>("companyInfoId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TermsAcceptedId");

                    b.HasIndex("companyInfoId");

                    b.ToTable("users");
                });

            modelBuilder.Entity("CoachOnline.Model.UserBillingPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<DateTime?>("ActivationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("BillingPlanTypeId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsStudent")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("PlannedActivationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<byte>("Status")
                        .HasColumnType("smallint");

                    b.Property<string>("StripePriceId")
                        .HasColumnType("text");

                    b.Property<string>("StripeProductId")
                        .HasColumnType("text");

                    b.Property<string>("StripeSubscriptionId")
                        .HasColumnType("text");

                    b.Property<string>("StripeSubscriptionScheduleId")
                        .HasColumnType("text");

                    b.Property<byte>("StudentCardVerificationStatus")
                        .HasColumnType("smallint");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BillingPlanTypeId");

                    b.HasIndex("UserId");

                    b.ToTable("UserBillingPlans");
                });

            modelBuilder.Entity("CoachOnline.Model.UserLogins", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<string>("AuthToken")
                        .HasColumnType("text");

                    b.Property<long>("Created")
                        .HasColumnType("bigint");

                    b.Property<string>("DeviceInfo")
                        .HasColumnType("text");

                    b.Property<bool>("Disposed")
                        .HasColumnType("boolean");

                    b.Property<string>("IpAddress")
                        .HasColumnType("text");

                    b.Property<string>("PlaceInfo")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<long>("ValidTo")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("userLogins");
                });

            modelBuilder.Entity("CoachOnline.Model.UserStudentCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<string>("StudentsCardPhotoName")
                        .HasColumnType("text");

                    b.Property<int>("UserBillingPlanId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserBillingPlanId");

                    b.ToTable("UserStudentCard");
                });

            modelBuilder.Entity("CoachOnline.Model.UserWatchedEpisode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseSerialColumn();

                    b.Property<DateTime>("Day")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("EpisodeDuration")
                        .HasColumnType("numeric");

                    b.Property<int>("EpisodeId")
                        .HasColumnType("integer");

                    b.Property<decimal>("EpisodeWatchedTime")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsWatched")
                        .HasColumnType("boolean");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("UserWatchedEpisodes");
                });

            modelBuilder.Entity("CoachOnline.Model.AdminLogin", b =>
                {
                    b.HasOne("CoachOnline.Model.Admin", null)
                        .WithMany("AdminLogins")
                        .HasForeignKey("AdminId");
                });

            modelBuilder.Entity("CoachOnline.Model.Category", b =>
                {
                    b.HasOne("CoachOnline.Model.Category", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.HasOne("CoachOnline.Model.User", null)
                        .WithMany("AccountCategories")
                        .HasForeignKey("UserId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("CoachOnline.Model.CoachBalanceDay", b =>
                {
                    b.HasOne("CoachOnline.Model.CoachBalanceMonth", "CoachBalanceMonth")
                        .WithMany("DayBalances")
                        .HasForeignKey("CoachBalanceMonthId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CoachBalanceMonth");
                });

            modelBuilder.Entity("CoachOnline.Model.CoachBalanceMonth", b =>
                {
                    b.HasOne("CoachOnline.Model.User", "Coach")
                        .WithMany()
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.MonthlyBalance", "MonthlyBalance")
                        .WithMany()
                        .HasForeignKey("MonthlyBalanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");

                    b.Navigation("MonthlyBalance");
                });

            modelBuilder.Entity("CoachOnline.Model.Course", b =>
                {
                    b.HasOne("CoachOnline.Model.Category", "Category")
                        .WithMany("CategoryCourses")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.User", "User")
                        .WithMany("OwnedCourses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CoachOnline.Model.Episode", b =>
                {
                    b.HasOne("CoachOnline.Model.Course", "Course")
                        .WithMany("Episodes")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("CoachOnline.Model.EpisodeAttachment", b =>
                {
                    b.HasOne("CoachOnline.Model.Episode", null)
                        .WithMany("Attachments")
                        .HasForeignKey("EpisodeId");
                });

            modelBuilder.Entity("CoachOnline.Model.FlaggedCourse", b =>
                {
                    b.HasOne("CoachOnline.Model.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("CoachOnline.Model.Rejection", b =>
                {
                    b.HasOne("CoachOnline.Model.Course", null)
                        .WithMany("RejectionsHistory")
                        .HasForeignKey("CourseId");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentCardRejection", b =>
                {
                    b.HasOne("CoachOnline.Model.UserBillingPlan", "Subscription")
                        .WithOne("StudentCardRejection")
                        .HasForeignKey("CoachOnline.Model.StudentCardRejection", "SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subscription");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentCourse", b =>
                {
                    b.HasOne("CoachOnline.Model.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.User", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentEpisode", b =>
                {
                    b.HasOne("CoachOnline.Model.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.Episode", "Episode")
                        .WithMany()
                        .HasForeignKey("EpisodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.StudentCourse", null)
                        .WithMany("StudentEpisodes")
                        .HasForeignKey("StudentCourseId");

                    b.HasOne("CoachOnline.Model.User", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Episode");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("CoachOnline.Model.SubscriptionPrice", b =>
                {
                    b.HasOne("CoachOnline.Model.BillingPlan", null)
                        .WithOne("Price")
                        .HasForeignKey("CoachOnline.Model.SubscriptionPrice", "BillingPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CoachOnline.Model.TwoFATokens", b =>
                {
                    b.HasOne("CoachOnline.Model.User", null)
                        .WithMany("TwoFATokens")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("CoachOnline.Model.User", b =>
                {
                    b.HasOne("CoachOnline.Model.Terms", "TermsAccepted")
                        .WithMany()
                        .HasForeignKey("TermsAcceptedId");

                    b.HasOne("CoachOnline.Model.CompanyInfo", "companyInfo")
                        .WithMany()
                        .HasForeignKey("companyInfoId");

                    b.Navigation("companyInfo");

                    b.Navigation("TermsAccepted");
                });

            modelBuilder.Entity("CoachOnline.Model.UserBillingPlan", b =>
                {
                    b.HasOne("CoachOnline.Model.BillingPlan", "BillingPlanType")
                        .WithMany("UserBillingPlans")
                        .HasForeignKey("BillingPlanTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoachOnline.Model.User", "User")
                        .WithMany("UserBillingPlans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BillingPlanType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CoachOnline.Model.UserLogins", b =>
                {
                    b.HasOne("CoachOnline.Model.User", null)
                        .WithMany("UserLogins")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("CoachOnline.Model.UserStudentCard", b =>
                {
                    b.HasOne("CoachOnline.Model.UserBillingPlan", "UserBillingPlan")
                        .WithMany("StudentCardData")
                        .HasForeignKey("UserBillingPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserBillingPlan");
                });

            modelBuilder.Entity("CoachOnline.Model.Admin", b =>
                {
                    b.Navigation("AdminLogins");
                });

            modelBuilder.Entity("CoachOnline.Model.BillingPlan", b =>
                {
                    b.Navigation("Price");

                    b.Navigation("UserBillingPlans");
                });

            modelBuilder.Entity("CoachOnline.Model.Category", b =>
                {
                    b.Navigation("CategoryCourses");

                    b.Navigation("Children");
                });

            modelBuilder.Entity("CoachOnline.Model.CoachBalanceMonth", b =>
                {
                    b.Navigation("DayBalances");
                });

            modelBuilder.Entity("CoachOnline.Model.Course", b =>
                {
                    b.Navigation("Episodes");

                    b.Navigation("RejectionsHistory");
                });

            modelBuilder.Entity("CoachOnline.Model.Episode", b =>
                {
                    b.Navigation("Attachments");
                });

            modelBuilder.Entity("CoachOnline.Model.StudentCourse", b =>
                {
                    b.Navigation("StudentEpisodes");
                });

            modelBuilder.Entity("CoachOnline.Model.User", b =>
                {
                    b.Navigation("AccountCategories");

                    b.Navigation("OwnedCourses");

                    b.Navigation("TwoFATokens");

                    b.Navigation("UserBillingPlans");

                    b.Navigation("UserLogins");
                });

            modelBuilder.Entity("CoachOnline.Model.UserBillingPlan", b =>
                {
                    b.Navigation("StudentCardData");

                    b.Navigation("StudentCardRejection");
                });
#pragma warning restore 612, 618
        }
    }
}
