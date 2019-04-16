using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Rancher.Community.Slack.Data.Models;

namespace Rancher.Community.Slack.Data
{
    public partial class SlackTrackerContext : DbContext
    {
        public SlackTrackerContext()
        {
        }

        public SlackTrackerContext(DbContextOptions<SlackTrackerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Channels> Channels { get; set; }
        public virtual DbSet<MessageHistory> MessageHistory { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<UserJoinedGeneralEvents> UserJoinedGeneralEvents { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString =
                    Environment.GetEnvironmentVariable("db_connection", EnvironmentVariableTarget.Process);
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.2-servicing-10034");

            modelBuilder.Entity<Channels>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IDX_Channels_Name");

                entity.Property(e => e.Id)
                    .HasMaxLength(255)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<MessageHistory>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(255)
                    .ValueGeneratedNever();

                entity.Property(e => e.Channel)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.HasIndex(e => e.Channel)
                    .HasName("IDX_Messages_Channel");

                entity.HasIndex(e => e.User)
                    .HasName("IDX_Messages_User");

                entity.Property(e => e.EventId)
                    .HasMaxLength(255)
                    .ValueGeneratedNever();

                entity.Property(e => e.Channel)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<UserJoinedGeneralEvents>(entity =>
            {
                entity.HasKey(e => e.User);

                entity.Property(e => e.User)
                    .HasMaxLength(255)
                    .ValueGeneratedNever();

                entity.Property(e => e.HasBeenWelcomed).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("IDX_Users_Name");

                entity.Property(e => e.Id)
                    .HasMaxLength(255)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Username).HasMaxLength(255);

                entity.Property(e => e.EmailAddress).HasMaxLength(1000);

                entity.Property(e => e.Timezone).HasMaxLength(255);

                entity.Property(e => e.TimezoneLabel).HasMaxLength(255);

                entity.Property(e => e.ImagePath).HasMaxLength(2083);
            });
        }
    }
}
