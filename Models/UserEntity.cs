using Azure.Data.Tables;
using Azure;

namespace DoctorCeo.Models;

public class UserEntity : ITableEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string NameId { get; set; }
    public string SigninProvider { get; set; }
    public string LastSigninDate { get; set; }
    //ITableEntity Members
    public virtual string PartitionKey { get => SigninProvider.ToString(); set => SigninProvider = value; }
    public virtual string RowKey { get => Email.ToString(); set => Email = value; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public UserEntity() { }
    // public UserEntity(string signinProvider, string lastSigninDate,string name)
    // {
    //     PartitionKey = signinProvider;
    //     RowKey = lastSigninDate;
    //     Name = name;
    // }
}
