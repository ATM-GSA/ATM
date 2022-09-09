<h2>Welcome to Environment and Climate Change Canada’s open-source repository for the Application Tracking Manager (ATM).</h2>

<p>The Application Tracking Manager project began as a research and development initiative intended to explore Blazor for future use within ECCC’s Reusable Applications Division (RAD). It is now relied upon by several development teams, and provides a centralized and authoritative resource for managing information about the RAD’s software applications.<br>

ATM utilizes <a href="https://github.com/ant-design-blazor">Ant Design Blazor</a> as its front-end component library and <a href="https://github.com/dotnet/blazor">Blazor Server</a> for the backend. With a focus on modular design, ATM provides users with a significant amount of customizability for the applications stored in the system. The modular design was chosen specifically to provide an excellent user experience, improve project maintainability, and reduce the overhead required when reporting modules need to be added or removed from the system.<br>

ATM is designed, developed, and supported by a team of student developers and has become an invaluable tool for ECCC’s Reusable Applications Division. The project will continue to receive future updates and each major release will be merged to this open-source repository.</p>

 <img src="https://szcz.dev/files/atm.png" alt="ATM"> 

<h3>Getting Started</h3>

ATM uses a SQL Server database along with EF Core. You will need to create an instance of SQL Server to launch the application. <br>

Once a database instance is available, add the connection string to `appsettings.json` and supply this connection to `line 29` in `Startup.cs`. Additionally, if you would like to run the test suite, a connection to a database is required in the TABSTest project.

<h3>Migrate data model</h3>

First, begin by installing the dotnet tool from the command line

`dotnet tool install --global dotnet-ef`

Next, create an EF Core migration

`dotnet ef migrations add InitializeDB`

Finally, run the migration

`dotnet ef database update`

<h3>Set up Auth</h3>

The original ATM project uses Active Directory authentication and most of the code has been redacted as a precaution. If AD auth is chosen, you can add your own code to fetch user records from the database in `ClaimsTransformer.cs`

Once your auth has been set up, you can launch the application. After registering your first user account, approve the account from the DB and give this user admin privileges. After this, additional user registrations can be approved from the admin panel.

