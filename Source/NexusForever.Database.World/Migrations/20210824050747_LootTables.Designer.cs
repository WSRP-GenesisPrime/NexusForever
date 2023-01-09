﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NexusForever.Database.World;

namespace NexusForever.Database.World.Migrations
{
    [DbContext(typeof(WorldContext))]
    [Migration("20210824050747_LootTables")]
    partial class LootTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("NexusForever.Database.World.Model.DisableModel", b =>
                {
                    b.Property<byte>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("type");

                    b.Property<uint>("ObjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("objectId");

                    b.Property<string>("Note")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(500)")
                        .HasDefaultValue("")
                        .HasColumnName("note");

                    b.HasKey("Type", "ObjectId")
                        .HasName("PRIMARY");

                    b.ToTable("disable");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityLootModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<ulong?>("LootGroupId")
                        .HasColumnType("bigint(20) unsigned")
                        .HasColumnName("lootGroupId");

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(200)")
                        .HasDefaultValue("")
                        .HasColumnName("comment");

                    b.HasKey("Id", "LootGroupId")
                        .HasName("PRIMARY");

                    b.HasIndex("LootGroupId");

                    b.ToTable("entity_loot");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<ulong>("ActivePropId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint(20) unsigned")
                        .HasDefaultValue(0ul)
                        .HasColumnName("activePropId");

                    b.Property<ushort>("Area")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("area");

                    b.Property<uint>("Creature")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("creature");

                    b.Property<uint>("DisplayInfo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("displayInfo");

                    b.Property<ushort>("Faction1")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("faction1");

                    b.Property<ushort>("Faction2")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("faction2");

                    b.Property<ushort>("OutfitInfo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("outfitInfo");

                    b.Property<byte>("QuestChecklistIdx")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("questChecklistIdx");

                    b.Property<float>("Rx")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("rx");

                    b.Property<float>("Ry")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("ry");

                    b.Property<float>("Rz")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("rz");

                    b.Property<byte>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("type");

                    b.Property<ushort>("World")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("world");

                    b.Property<ushort>("WorldSocketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("worldSocketId");

                    b.Property<float>("X")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("x");

                    b.Property<float>("Y")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("y");

                    b.Property<float>("Z")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("z");

                    b.HasKey("Id");

                    b.ToTable("entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntitySplineModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<float>("Fx")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("fx");

                    b.Property<float>("Fy")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("fy");

                    b.Property<float>("Fz")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("fz");

                    b.Property<byte>("Mode")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("mode");

                    b.Property<float>("Speed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("speed");

                    b.Property<ushort>("SplineId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("splineId");

                    b.HasKey("Id");

                    b.ToTable("entity_spline");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityStatModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<byte>("Stat")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("stat");

                    b.Property<float>("Value")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("value");

                    b.HasKey("Id", "Stat")
                        .HasName("PRIMARY");

                    b.ToTable("entity_stats");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorCategoryModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<uint>("Index")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("index");

                    b.Property<uint>("LocalisedTextId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("localisedTextId");

                    b.HasKey("Id", "Index")
                        .HasName("PRIMARY");

                    b.ToTable("entity_vendor_category");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorItemModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<uint>("Index")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("index");

                    b.Property<uint>("CategoryIndex")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("categoryIndex");

                    b.Property<uint>("ItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("itemId");

                    b.HasKey("Id", "Index")
                        .HasName("PRIMARY");

                    b.ToTable("entity_vendor_item");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<float>("BuyPriceMultiplier")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(1f)
                        .HasColumnName("buyPriceMultiplier");

                    b.Property<float>("SellPriceMultiplier")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(1f)
                        .HasColumnName("sellPriceMultiplier");

                    b.HasKey("Id");

                    b.ToTable("entity_vendor");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.ItemLootModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<ulong?>("LootGroupId")
                        .HasColumnType("bigint(20) unsigned")
                        .HasColumnName("lootGroupId");

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(200)")
                        .HasDefaultValue("")
                        .HasColumnName("comment");

                    b.HasKey("Id", "LootGroupId")
                        .HasName("PRIMARY");

                    b.HasIndex("LootGroupId");

                    b.ToTable("item_loot");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.LootGroupModel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint(20) unsigned")
                        .HasDefaultValue(0ul)
                        .HasColumnName("id");

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(200)")
                        .HasDefaultValue("")
                        .HasColumnName("comment");

                    b.Property<uint>("Condition")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("condition");

                    b.Property<uint>("ConditionType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("conditionType");

                    b.Property<uint>("MaxDrop")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("maxDrop");

                    b.Property<uint>("MinDrop")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("minDrop");

                    b.Property<ulong?>("ParentId")
                        .HasColumnType("bigint(20) unsigned")
                        .HasColumnName("parentId");

                    b.Property<float>("Probability")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(100f)
                        .HasColumnName("probability");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("loot_group");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.LootItemModel", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint(20) unsigned")
                        .HasDefaultValue(0ul)
                        .HasColumnName("id");

                    b.Property<uint>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("type");

                    b.Property<uint>("StaticId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("staticId");

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(200)")
                        .HasDefaultValue("")
                        .HasColumnName("comment");

                    b.Property<uint>("MaxCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("maxCount");

                    b.Property<uint>("MinCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("minCount");

                    b.Property<float>("Probability")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(100f)
                        .HasColumnName("probability");

                    b.HasKey("Id", "Type", "StaticId")
                        .HasName("PRIMARY");

                    b.ToTable("loot_item");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreCategoryModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(150)")
                        .HasDefaultValue("")
                        .HasColumnName("description");

                    b.Property<uint>("Index")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(1u)
                        .HasColumnName("index");

                    b.Property<string>("Name")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("")
                        .HasColumnName("name");

                    b.Property<uint>("ParentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(26u)
                        .HasColumnName("parentId");

                    b.Property<byte>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("visible");

                    b.HasKey("Id");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("parentId");

                    b.ToTable("store_category");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferGroupCategoryModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<uint>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("categoryId");

                    b.Property<byte>("Index")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("index");

                    b.Property<byte>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1) unsigned")
                        .HasDefaultValue((byte)1)
                        .HasColumnName("visible");

                    b.HasKey("Id", "CategoryId")
                        .HasName("PRIMARY");

                    b.HasIndex("CategoryId")
                        .HasDatabaseName("FK__store_offer_group_category_categoryId__store_category_id");

                    b.ToTable("store_offer_group_category");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferGroupModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(500)")
                        .HasDefaultValue("")
                        .HasColumnName("description");

                    b.Property<uint>("DisplayFlags")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("displayFlags");

                    b.Property<ushort>("DisplayInfoOverride")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("displayInfoOverride");

                    b.Property<string>("Name")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("")
                        .HasColumnName("name");

                    b.Property<byte>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("visible");

                    b.HasKey("Id");

                    b.ToTable("store_offer_group");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemDataModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<ushort>("ItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint(5) unsigned")
                        .HasDefaultValue((ushort)0)
                        .HasColumnName("itemId");

                    b.Property<uint>("Amount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(1u)
                        .HasColumnName("amount");

                    b.Property<uint>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("type");

                    b.HasKey("Id", "ItemId")
                        .HasName("PRIMARY");

                    b.ToTable("store_offer_item_data");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<uint>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("groupId");

                    b.Property<string>("Description")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(500)")
                        .HasDefaultValue("")
                        .HasColumnName("description");

                    b.Property<uint>("DisplayFlags")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("displayFlags");

                    b.Property<long>("Field6")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint(20)")
                        .HasDefaultValue(0L)
                        .HasColumnName("field_6");

                    b.Property<byte>("Field7")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("field_7");

                    b.Property<string>("Name")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("")
                        .HasColumnName("name");

                    b.Property<byte>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("visible");

                    b.HasKey("Id", "GroupId")
                        .HasName("PRIMARY");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("FK__store_offer_item_groupId__store_offer_group_id");

                    b.HasIndex("Id")
                        .IsUnique()
                        .HasDatabaseName("id");

                    b.ToTable("store_offer_item");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemPriceModel", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id");

                    b.Property<byte>("CurrencyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("currencyId");

                    b.Property<byte>("DiscountType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(3) unsigned")
                        .HasDefaultValue((byte)0)
                        .HasColumnName("discountType");

                    b.Property<float>("DiscountValue")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("discountValue");

                    b.Property<long>("Expiry")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint(20)")
                        .HasDefaultValue(0L)
                        .HasColumnName("expiry");

                    b.Property<long>("Field14")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint(20)")
                        .HasDefaultValue(0L)
                        .HasColumnName("field_14");

                    b.Property<float>("Price")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0f)
                        .HasColumnName("price");

                    b.HasKey("Id", "CurrencyId")
                        .HasName("PRIMARY");

                    b.ToTable("store_offer_item_price");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.TutorialModel", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("id")
                        .HasComment("Tutorial ID");

                    b.Property<uint>("Type")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("type");

                    b.Property<uint>("TriggerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(10) unsigned")
                        .HasDefaultValue(0u)
                        .HasColumnName("triggerId");

                    b.Property<string>("Note")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(50)")
                        .HasDefaultValue("")
                        .HasColumnName("note");

                    b.HasKey("Id", "Type", "TriggerId")
                        .HasName("PRIMARY");

                    b.ToTable("tutorial");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityLootModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.LootGroupModel", "LootGroup")
                        .WithMany()
                        .HasForeignKey("LootGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LootGroup");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntitySplineModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.EntityModel", "Entity")
                        .WithOne("EntitySpline")
                        .HasForeignKey("NexusForever.Database.World.Model.EntitySplineModel", "Id")
                        .HasConstraintName("FK__entity_spline_id__entity_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityStatModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.EntityModel", "Entity")
                        .WithMany("EntityStat")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__entity_stats_stat_id_entity_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorCategoryModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.EntityModel", "Entity")
                        .WithMany("EntityVendorCategory")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__entity_vendor_category_id__entity_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorItemModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.EntityModel", "Entity")
                        .WithMany("EntityVendorItem")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__entity_vendor_item_id__entity_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityVendorModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.EntityModel", "Entity")
                        .WithOne("EntityVendor")
                        .HasForeignKey("NexusForever.Database.World.Model.EntityVendorModel", "Id")
                        .HasConstraintName("FK__entity_vendor_id__entity_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.ItemLootModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.LootGroupModel", "LootGroup")
                        .WithMany()
                        .HasForeignKey("LootGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LootGroup");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.LootGroupModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.LootGroupModel", "Parent")
                        .WithMany("ChildGroup")
                        .HasForeignKey("ParentId")
                        .HasConstraintName("FK__loot_group_parentId__loot_group_id");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.LootItemModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.LootGroupModel", "LootGroup")
                        .WithMany("Item")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__loot_item_id__loot_group_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LootGroup");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferGroupCategoryModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.StoreCategoryModel", "Category")
                        .WithMany("StoreOfferGroupCategory")
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("FK__store_offer_group_category_categoryId__store_category_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NexusForever.Database.World.Model.StoreOfferGroupModel", "OfferGroup")
                        .WithMany("StoreOfferGroupCategory")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__store_offer_group_category_id__store_offer_group_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("OfferGroup");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemDataModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.StoreOfferItemModel", "OfferItem")
                        .WithMany("StoreOfferItemData")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__store_offer_item_data_id__store_offer_item_id")
                        .HasPrincipalKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OfferItem");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.StoreOfferGroupModel", "Group")
                        .WithMany("StoreOfferItem")
                        .HasForeignKey("GroupId")
                        .HasConstraintName("FK__store_offer_item_groupId__store_offer_group_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemPriceModel", b =>
                {
                    b.HasOne("NexusForever.Database.World.Model.StoreOfferItemModel", "OfferItem")
                        .WithMany("StoreOfferItemPrice")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK__store_offer_item_price_id__store_offer_item_id")
                        .HasPrincipalKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OfferItem");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.EntityModel", b =>
                {
                    b.Navigation("EntitySpline");

                    b.Navigation("EntityStat");

                    b.Navigation("EntityVendor");

                    b.Navigation("EntityVendorCategory");

                    b.Navigation("EntityVendorItem");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.LootGroupModel", b =>
                {
                    b.Navigation("ChildGroup");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreCategoryModel", b =>
                {
                    b.Navigation("StoreOfferGroupCategory");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferGroupModel", b =>
                {
                    b.Navigation("StoreOfferGroupCategory");

                    b.Navigation("StoreOfferItem");
                });

            modelBuilder.Entity("NexusForever.Database.World.Model.StoreOfferItemModel", b =>
                {
                    b.Navigation("StoreOfferItemData");

                    b.Navigation("StoreOfferItemPrice");
                });
#pragma warning restore 612, 618
        }
    }
}
