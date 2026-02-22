using System;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Domain.Entities;

/// <summary>
/// Base entity class that provides common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// Using Guid for distributed systems and security (vs auto-increment int)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user who created the entity
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the entity
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Check if the entity is newly created (not persisted yet)
    /// </summary>
    public bool IsTransient() => Id == Guid.Empty;

    /// <summary>
    /// Update the audit fields when entity is modified
    /// </summary>
    public void UpdateAuditFields(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy ?? UpdatedBy;
    }

    /// <summary>
    /// Override equality comparison to use Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (IsTransient() || other.IsTransient())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Override hash code to use Id
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Override ToString to show entity type and Id
    /// </summary>
    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}