using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lifeline.DatabaseContext;

internal class LifelineContext
{
    [Key] public required int Key { set; get; }
    
    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Name { set; get; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Target { set; get; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(40)]
    public string? Id { set; get; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public required string Token { set; get; }

    public required DateTime LastInsert { set; get; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(255)]
    public string? Path { set; get; }

    [Column(TypeName = "VARCHAR")]
    [StringLength(4096)]
    public string? Data { set; get; }
}