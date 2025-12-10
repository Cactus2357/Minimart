# Common DB Connection
# dotnet add package Microsoft.Extensions.Configuration.Json;
# dotnet add package Microsoft.Extensions.Configuration;

dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.11;
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.11;
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.11;
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.11;
dotnet add package Microsoft.AspNetCore.Identity.UI --version 9.0.11;
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.11;
dotnet add package Microsoft.AspNetCore.OData --version 9.4.1;
dotnet add package Microsoft.OData.ModelBuilder;
dotnet add package Swashbuckle.AspNetCore --version 6.5.0;

# JWT (net9.0)
# dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer;
# dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore;
# dotnet add package Microsoft.AspNetCore.Identity.UI;

# dotnet ef dbcontext scaffold "server=DOMN; database=PE_PRN_25FallB1; uid=sa; pwd=123; TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --force;

# Publish
# dotnet publish <directory> -c release -o <output_directory>

# dotnet ef migrations add "InitialDB";
# dotnet ef database update