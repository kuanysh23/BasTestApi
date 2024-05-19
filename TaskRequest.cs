using System.ComponentModel.DataAnnotations;

namespace BasTestApi;

public class TaskRequest
{
    [MaxLength(100)]
    public string Message { get; set; }
}