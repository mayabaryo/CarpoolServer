using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace CarpoolServerBL.Models
{
    public partial class CarpoolDBContext : DbContext
    {
        public CarpoolDBContext()
        {
        }

        public CarpoolDBContext(DbContextOptions<CarpoolDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<Adult> Adults { get; set; }
        public virtual DbSet<Carpool> Carpools { get; set; }
        public virtual DbSet<CarpoolStatus> CarpoolStatuses { get; set; }
        public virtual DbSet<Kid> Kids { get; set; }
        public virtual DbSet<KidsInActivity> KidsInActivities { get; set; }
        public virtual DbSet<KidsInCarpool> KidsInCarpools { get; set; }
        public virtual DbSet<KidsOfAdult> KidsOfAdults { get; set; }
        public virtual DbSet<RequestCarpoolStatus> RequestCarpoolStatuses { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=CarpoolDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ActivityName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.AdultId).HasColumnName("AdultID");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(d => d.Adult)
                    .WithMany(p => p.Activities)
                    .HasForeignKey(d => d.AdultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Activitie__Adult__30F848ED");
            });

            modelBuilder.Entity<Adult>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Adult)
                    .HasForeignKey<Adult>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Adults__ID__286302EC");
            });

            modelBuilder.Entity<Carpool>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ActivityId).HasColumnName("ActivityID");

                entity.Property(e => e.AdultId).HasColumnName("AdultID");

                entity.Property(e => e.CarpoolStatusId).HasColumnName("CarpoolStatusID");

                entity.Property(e => e.CarpoolTime).HasColumnType("datetime");

                entity.HasOne(d => d.Activity)
                    .WithMany(p => p.Carpools)
                    .HasForeignKey(d => d.ActivityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Carpools__Activi__3A81B327");

                entity.HasOne(d => d.Adult)
                    .WithMany(p => p.Carpools)
                    .HasForeignKey(d => d.AdultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Carpools__AdultI__38996AB5");

                entity.HasOne(d => d.CarpoolStatus)
                    .WithMany(p => p.Carpools)
                    .HasForeignKey(d => d.CarpoolStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Carpools__Carpoo__398D8EEE");
            });

            modelBuilder.Entity<CarpoolStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId)
                    .HasName("PK__CarpoolS__C8EE204386F21EC1");

                entity.ToTable("CarpoolStatus");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.StatusName)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<Kid>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Kid)
                    .HasForeignKey<Kid>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Kids__ID__2B3F6F97");
            });

            modelBuilder.Entity<KidsInActivity>(entity =>
            {
                entity.HasKey(e => new { e.KidId, e.ActivityId });

                entity.Property(e => e.KidId).HasColumnName("KidID");

                entity.Property(e => e.ActivityId).HasColumnName("ActivityID");

                entity.HasOne(d => d.Activity)
                    .WithMany(p => p.KidsInActivities)
                    .HasForeignKey(d => d.ActivityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsInAct__Activ__33D4B598");

                entity.HasOne(d => d.Kid)
                    .WithMany(p => p.KidsInActivities)
                    .HasForeignKey(d => d.KidId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsInAct__KidID__32E0915F");
            });

            modelBuilder.Entity<KidsInCarpool>(entity =>
            {
                entity.HasKey(e => new { e.KidId, e.CarpoolId });

                entity.Property(e => e.KidId).HasColumnName("KidID");

                entity.Property(e => e.CarpoolId).HasColumnName("CarpoolID");

                entity.Property(e => e.KidOnBoard).HasDefaultValueSql("((0))");

                entity.Property(e => e.StatusId)
                    .HasColumnName("StatusID")
                    .HasDefaultValueSql("((3))");

                entity.HasOne(d => d.Carpool)
                    .WithMany(p => p.KidsInCarpools)
                    .HasForeignKey(d => d.CarpoolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsInCar__Carpo__3D5E1FD2");

                entity.HasOne(d => d.Kid)
                    .WithMany(p => p.KidsInCarpools)
                    .HasForeignKey(d => d.KidId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsInCar__KidID__3C69FB99");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.KidsInCarpools)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsInCar__Statu__6EF57B66");
            });

            modelBuilder.Entity<KidsOfAdult>(entity =>
            {
                entity.HasKey(e => new { e.AdultId, e.KidId });

                entity.Property(e => e.AdultId).HasColumnName("AdultID");

                entity.Property(e => e.KidId).HasColumnName("KidID");

                entity.HasOne(d => d.Adult)
                    .WithMany(p => p.KidsOfAdults)
                    .HasForeignKey(d => d.AdultId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsOfAdu__Adult__2D27B809");

                entity.HasOne(d => d.Kid)
                    .WithMany(p => p.KidsOfAdults)
                    .HasForeignKey(d => d.KidId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__KidsOfAdu__KidID__2E1BDC42");
            });

            modelBuilder.Entity<RequestCarpoolStatus>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__RequestC__33A8519A56381E9C");

                entity.ToTable("RequestCarpoolStatus");

                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.RequestName)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email, "UC_Email")
                    .IsUnique();

                entity.HasIndex(e => e.UserName, "UC_UserName")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BirthDate).HasColumnType("datetime");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.PhoneNum)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Photo)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.UserPswd)
                    .IsRequired()
                    .HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
