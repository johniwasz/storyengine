//using System;
//using System.Collections.Generic;

//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Whetstone.StoryEngine.Models;
//using Whetstone.StoryEngine.Models.Actions;
//using Whetstone.StoryEngine.Models.Conditions;
//using Whetstone.StoryEngine.Models.Data;
//using Whetstone.StoryEngine.Models.Story;
//using Whetstone.StoryEngine.Models.Story.Ssml;
//using Whetstone.StoryEngine.Models.Story.Text;
//using Whetstone.StoryEngine.Models.Tracking;


//namespace Whetstone.StoryEngine.Data
//{
//    public class StoryEngineContext :DbContext
//    {
//        private string _connectionString;

//        public DbSet<DataStory> StoryTitles { get; set; }

//        public DbSet<DataStoryVersion> StoryVersions { get; set; }

//        public DbSet<DataSlotType> SlotTypes { get; set; }

//        public DbSet<DataIntent> Intents { get; set; }

//        public DbSet<DataInventoryItem> InventoryItems { get; set; }

//        public DbSet<DataInventoryCondition> InventoryConditions { get; set; }

//        public DbSet<DataInventoryConditionXRef> InventoryConditionXRefs { get; set; }
        
//        public DbSet<DataIntentSlotMapping> MappedIntents { get; set; }

////        public DbSet<MappedSlot> MappedSlots { get; set; }

// //       public DbSet<MappedUtterance> MappedUtterances { get; set; }
 
//        public DbSet<DataChoice> Choices { get; set; }

//        public DbSet<InventoryActionData> InventoryActions { get; set; }

//        public DbSet<NodeVisitRecordActionData> NodeVisitActions { get; set; }

//        public DbSet<FragmentNodeVisitConditionXRef> FragmentNodeVisitConditionXRefs { get; set; }
        



//        public StoryEngineContext(DbContextOptions<StoryEngineContext> options) : base(options)
//        {

//        }


//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {

//            //optionsBuilder.UseLoggerFactory()
//            optionsBuilder.EnableSensitiveDataLogging();

//            base.OnConfiguring(optionsBuilder);
//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.HasDefaultSchema("public");

        
//            modelBuilder.Entity<NodeVisitRecordActionData>().HasBaseType<NodeActionData>();
//            modelBuilder.Entity<InventoryActionData>().HasBaseType<NodeActionData>();
//            modelBuilder.Entity<RecordSelectedItemActionData>().HasBaseType<NodeActionData>();

//            modelBuilder.Entity<NodeActionData>()
//                .HasDiscriminator<int>("NodeActionType")
//                .HasValue<NodeVisitRecordActionData>(1)
//                .HasValue<InventoryActionData>(2)
//                .HasValue<RecordSelectedItemActionData>(3);



//            //  modelBuilder.ComplexType<Coordinates>();

//            modelBuilder.Entity<DataNodeVisitConditionXRef>()
//                .HasOne(nv => nv.Condition)
//                .WithMany("NodeVisitConditionXRefs");

//            modelBuilder.Entity<DataNodeVisitConditionXRef>()
//                .HasOne(nv => nv.Node)
//                .WithMany("NodeVisitConditionXRefs");


//            modelBuilder.Entity<DataNodeVisitConditionXRef>()
//                .HasKey(t => new { t.NodeId, t.ConditionId });

//            //modelBuilder.Entity<FragmentInventoryConditionXRef>()
//            //    .HasOne(fi => fi.ConditionFragment)
//            //    .WithMany("FragmentInventoryConditionXRefs");

//            //modelBuilder.Entity<FragmentInventoryConditionXRef>()
//            //    .HasOne(fi => fi.Condition)
//            //    .WithMany("FragmentInventoryConditionXRefs");


//            //modelBuilder.Entity<FragmentNodeVisitConditionXRef>()
//            //    .HasOne(fi => fi.ConditionFragment)
//            //    .WithMany(x => x.);


//            //modelBuilder.Entity<FragmentNodeVisitConditionXRef>()
//            //    .HasOne(fi => fi.Condition)
//            //    .WithMany("FragmentNodeVisitConditionXRefs");

//            modelBuilder.Entity<FragmentNodeVisitConditionXRef>()
//             .HasKey(t => new { t.ConditionFragmentId, t.ConditionId });

//            modelBuilder.Entity<DataIntent>()
//                .HasIndex(di => di.UniqueId)
//                .IsUnique();

//            modelBuilder.Entity<DataSlotType>()
//                .HasIndex(st => st.UniqueId)
//                .IsUnique();

//            modelBuilder.Entity<DataSlotType>()
//                .HasIndex(st => new { st.Name, st.VersionId } )
//                .HasName("IX_SlotType_UniqueName")
//                .IsUnique();

//            modelBuilder.Entity<DataSlotType>()
//               .Property("ValuesJson");

//            modelBuilder.Entity<DataIntent>()
//                .Property("LocalizedIntentsJson");

//            //modelBuilder.Entity<FragmentInventoryConditionXRef>()
//            //    .HasOne(fi => fi.ConditionFragment)
//            //    .WithMany("FragmentInventoryConditionXRefs");

//            //modelBuilder.Entity<FragmentInventoryConditionXRef>()
//            //    .HasOne(fi => fi.Condition)
//            //    .WithMany("FragmentInventoryConditionXRefs");

//            modelBuilder.Entity<FragmentInventoryConditionXRef>()
//             .HasKey(t => new { t.ConditionFragmentId, t.ConditionId });

//            modelBuilder.Entity<DataInventoryConditionXRef>()
//                .HasKey(t => new { t.InventoryItemId, t.ConditionId });

//            modelBuilder.Entity<DataStory>()
//                .HasIndex(x => x.ShortName)
//                .IsUnique();

//            modelBuilder.Entity<DataNode>()
//                .Property("CoordinatesJson");

//            modelBuilder.Entity<DataStory>()
//                .HasIndex(x => x.ShortName)
//                .IsUnique();

//            modelBuilder.Entity<DataStory>()
//                .Property("InvocationNamesJson");

//            modelBuilder.Entity<DataStory>()
//                .HasIndex(ds => ds.UniqueId)
//                .IsUnique();

//            modelBuilder.Entity<DataStoryVersion>()
//                .HasIndex(x => x.UniqueId)
//                .IsUnique();

//            modelBuilder.Entity<DataStoryVersion>()
//                .HasOne(s => s.Story)
//                .WithMany(n => n.Versions)
//                .HasForeignKey(fk => fk.StoryId)
//                .HasConstraintName("FK_Story_Versions");


//            modelBuilder.Entity<DataStoryVersion>()
//                .HasQueryFilter(x => !x.IsDeleted);

//            //    modelBuilder.Entity<DataIntentSlotMapping>()
//            //         .HasIndex(p => p.UniqueId);

//            modelBuilder.Entity<DataIntentSlotMapping>()
//                .HasKey(c => new { c.IntentId, c.SlotTypeId });

//            modelBuilder.Entity<DataIntentSlotMapping>()
//                .HasIndex(c => new {c.IntentId, c.SlotTypeId, c.Alias})
//                .IsUnique();

//            modelBuilder.Entity<DataInventoryCondition>()
//                .HasIndex(ic => new {ic.Name, ic.VersionId})
//                .IsUnique();

//            modelBuilder.Entity<ChoiceConditionVisitXRef>()
//                .HasKey(t => new { t.NodeVisitConditionId, t.ChoiceId });


//            modelBuilder.Entity<ChoiceConditionVisitXRef>()
//                .HasOne(nv => nv.Condition)
//                .WithMany(x => x.ChoiceConditionVisitXRefs);

//            modelBuilder.Entity<ChoiceConditionVisitXRef>()
//                .HasOne(nv => nv.Choice)
//                .WithMany(x => x.ChoiceConditionVisitXRefs);

//            modelBuilder.Entity<DataNode>()
//                .HasIndex(ind => new {ind.VersionId, ind.ChapterId, ind.Name})
//                .IsUnique();
            

//            modelBuilder.Entity<DataInventoryConditionXRef>()
//                .HasKey(bc => new { bc.ConditionId, bc.InventoryItemId });

//            modelBuilder.Entity<DataInventoryConditionXRef>()
//                .HasOne(bc => bc.Condition)
//                .WithMany(b => b.InventoryConditionXRefs)
//                .HasForeignKey(bc => bc.ConditionId);

//            modelBuilder.Entity<DataInventoryConditionXRef>()
//                .HasOne(bc => bc.InventoryItem)
//                .WithMany(c => c.InventoryConditionXRefs)
//                .HasForeignKey(bc => bc.InventoryItemId);

           


//            //modelBuilder.Entity<StoryNode>()
//            //    .HasOne(p => p.ParentTitle)
//            //    .WithMany(b => b.Nodes)
//            //    .HasForeignKey(p => p.ParentTitleId);                


//            //[Table("InventoryConditonXRefs")]


//            modelBuilder.Entity<SimpleTextFragment>().HasBaseType<TextFragmentBase>();
//            modelBuilder.Entity<ConditionalTextFragment>().HasBaseType<TextFragmentBase>();

//            modelBuilder.Entity<TextFragmentBase>()
//                .HasDiscriminator<int>("TxtFragmentType")
//                .HasValue<SimpleTextFragment>(1)
//                .HasValue<ConditionalTextFragment>(2);
               

//            modelBuilder.Entity<DataAudioFile>().HasBaseType<DataSpeechFragment>();
//            modelBuilder.Entity<DataDirectAudioFile>().HasBaseType<DataSpeechFragment>();
//            modelBuilder.Entity<DataSpeechText>().HasBaseType<DataSpeechFragment>();
//            modelBuilder.Entity<DataConditionalFragment>().HasBaseType<DataSpeechFragment>();
//            modelBuilder.Entity<DataSsmlSpeechFragment>().HasBaseType<DataSpeechFragment>();
//            modelBuilder.Entity<DataSpeechBreakFragment>().HasBaseType<DataSpeechFragment>();

//            modelBuilder.Entity<DataSpeechFragment>()
//                .HasDiscriminator<int>("SpFragmentType")
//                .HasValue<DataAudioFile>(1)
//                .HasValue<DataDirectAudioFile>(2)
//                .HasValue<DataSpeechText>(3)
//                .HasValue<DataConditionalFragment>(4)
//                .HasValue<DataSsmlSpeechFragment>(5)
//                .HasValue<DataSpeechBreakFragment>(6);
            

//            modelBuilder.Entity<DataSpeechFragment>(e =>
//            {
//                e.Property(x => x.VersionId).IsRequired();            
//            });
            


//            //modelBuilder.Entity<StoryNode>().
//            //    HasMany(x => x.Actions).
//            //    WithOne(a => a.ParentNode)
//            //    .HasForeignKey(fk => fk.Id).HasConstraintName("FK_Node_NodeAction");


//            //modelBuilder.Entity<NodeActionBase>()
//            //    .HasOne(o => o.ParentNode)
//            //    .WithMany(m => m.Actions)
//            //    .HasForeignKey(fk => fk.NodeId).HasConstraintName("FK_Node_NodeAction");


//            //modelBuilder.Entity<UniqueItem>().HasBaseType<InventoryItemBase>();
//            //  modelBuilder.Entity<MultiItem>().HasBaseType<InventoryItemBase>();


//            //        [MessagePack.Union(0, typeof(InventoryCondition))]
//            //        [MessagePack.Union(1, typeof(NodeVisitCondition))]
//            //public abstract class StoryConditionBase



//            //modelBuilder.Entity<NodeVisitCondition>().HasBaseType<StoryConditionBase>();

//            //modelBuilder.Entity<SingleNodeMapping>().HasBaseType<NodeMappingBase>();
//            //modelBuilder.Entity<MultiNodeMapping>().HasBaseType<NodeMappingBase>();
//            //modelBuilder.Entity<ConditionalNodeMapping>().HasBaseType<NodeMappingBase>();
//            //modelBuilder.Entity<SlotNodeMapping>().HasBaseType<NodeMappingBase>();
//            //modelBuilder.Entity<SlotMap>().HasBaseType<NodeMappingBase>();






//            base.OnModelCreating(modelBuilder);
//        }
//    }


//    //public class DesignTimeFactory : IDesignTimeDbContextFactory<StoryEngineContext>
//    //{
//    //    public StoryEngineContext CreateDbContext(string[] args)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //}
//}
