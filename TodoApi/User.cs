using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi;

public class User
{
    public int Id { get; set; }

    [Column("Name")]
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";
}