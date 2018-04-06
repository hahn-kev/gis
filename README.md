## Gis
[![Build status](https://gisthailand.visualstudio.com/_apis/public/build/definitions/30e4089a-2508-47ae-abc3-ba12087ff8ae/1/badge)](https://gisthailand.visualstudio.com/Gis/_build/index?definitionId=1)

This document should explain some of the following for the GIS office app
* [how](#server-setup) the production server is configured
* [what](#config-files) the config files do
* [how](#build-info) you might go about updating the server
* [how](#running-locally) to build/test the application before updating the server
* [what](#tech-used) technology is used

### Server Setup

The server was configured following this guide
https://docs.microsoft.com/en-us/aspnet/core/publishing/linuxproduction?tabs=aspnetcore2x
the application was installed in the `/var/aspnetcore/gis` folder on google cloud the office-asia machine

In that folder you'll find the binary files along with all the web files under the `wwwroot` folder,
there's also a `Log` folder that has all the logs. 

#### config files
2 files of interest are the 2 configuration files
1. `appsettings.json`
   * `BaseURL`, this is used when generating emails links
   * `ConnectionString` is how we connect to the database
   * `JWTSettings` these are used for authentication
     * `SecretKey` if this is changed then any existing logins will get invalidated
     and everyone will be forced to log in again,
     * `Issuer` and `Audience` are unimportant, they're just labels, again if they change 
     logging in again is required
   * `GoogleAPIKey` this is the key we use to access things like google drive, etc
   * `SentryDNS` this is the key of sorts that lets us post errors to the Sentry error reporting system
   * `Environment` mostly informational, used for error reports
   * `SendGridAPIKey` allows us to send email using sendgrid
   * `TemplateSettings`, each of these are the SendGrid template id's used when sending notification emails,
   they'll have to be changed to a new SendGrid account (see below)
   
2. `client_id.json`
    this file is auto-generated for Google OAuth, just click download
    from the Google OAuth screen to get this file, OAuth is what allows
    the application to do google logins

#### Database

The Postgres database also lives on this machine,
there's no special configuration done with it, if you need to connect to it externally
you'll want to modify the `allow-postgres` firewall rule in google cloud to allow connections directly.
Connection information can be taken from the `appsettings.json` file `ConnectionString` property
you can read up on other options at [Npgsql](http://www.npgsql.org/doc/connection-string-parameters.html)

If you need to interact with the database directly, modify the firewall settings, and use an application
like [DBForge](https://www.devart.com/dbforge/postgresql/studio/), take the connection info 
from the connection string (replace hostname with the domain used to access the site).

To Actually make changes or query the database you'll need to learn `SQL` you can find a decent tutorial
at [w3schools](https://www.w3schools.com/sql/)

#### Helpful commands

* reboot GIS application `sudo service kestrel-gis restart`
* update config file `sudo nano /var/aspnetcore/gis/appsettings.json` to close press ctrl + x,
the text editor is called `nano` search google for more help on how to use it

### Build Info

The following will explain how to build the application
(take the source code, and turn it into a usable application) on your 
computer, and then deploy to the production server,
alternatively, you could run the application on your own computer to test out changes

Souce code can be found at [github](https://github.com/hahn-kev/gis)
Build setup can be found here [VSTS](https://gisthailand.visualstudio.com/Gis/_build)

First, you'll need to install the following, a quick google search should help you
try and get around the minor version, if it's too much trouble then just get 
the same major version.
* .net core 2.1.1
* nodejs 9.2.0 (includes npm 5.5.1)

Once you've done this ensure you can access `dotnet` and `npm` from
the command line. If you get an error that it's not installed or recognized then you might need to restart as those 2 apps should both
be in your path and accessible from anywhere in the command line.

Once these are both installed you'll need to do the following:
* build the frontend
* build the backend
* copy the frontend resources to the back end
* publish the backend

there's a script that will do this all for you on windows called `build.bat`
just make sure nodejs and .net core are installed first or you might get errors.

#### Deploy to Server

once you've run the build script (it might take a couple minutes) there should be a folder
called `website` this will contain a full copy of the site, I recommend zipping
it up and copying it to the server, after that connect to the server via SSH and extract the zip file like so:
```shell
unzip -o website.zip -d /var/aspnetcore/gis/
sudo service kestrel-gis restart
```

the first command extracts the files to where they need to go
the second command restarts the server so it'll use the new files.

#### Running locally
you might want to run the server locally so you can test out a change.

the first thing you need to do is copy over the `appsettings.json` and `client_id.json`
files from the server to your local computer put them in the `Backend` folder.
Note, you'll need to update the connection string setting to point at the 
database running on the server, this will require access from your machine
so update the google cloud firewall as appropriate and change the host
in the connection string to the domain name of the office server.

Now open a command prompt in the `Backend` folder and run
`dotnet run` once it starts up it should say it's now listening on 
`http://localhost:1650` (if not update `Frontend/proxy.config.json`
and replace the above URL with the one output by the command), but don't go
there just yet.

Next, open a command prompt in the `Frontend` folder and run `npm start`,
once it's started it should tell you that you can go to
[`http://localhost:4201`](http://localhost:4201) to view the site. You'll be
prompted to log in as usual. If the google login doesn't work you might
need to change URL and domain authorization for API keys and OAuth client ids.


### Email sending

Email is sent using [SendGrid](https://app.sendgrid.com), there's very little setup required
however, if access is lost to the Sendgrid account that's been configured then a new one will need to be made
and an update will have to be made to the application (this might change in the future).

#### Email Templates

SendGrid works using transactional templates. If you need to configure a template, you go to their website,
configure a template and the template will have an ID.
I've made a couple templates already and put the ID into the config file.
That way, when an email needs to be sent, I send the template ID to SendGrid and it knows what email
template I'd like to use. These ID's are unique to the account. If a different SendGrid account is used,
then the ID's will be different and need to be updated.

If you need to modify the template for an email, you should be able to login to the SendGrid site
and find the template. The templates work using text substitutions, in the template they might look
like some of the following:
* `:firstName`
* `:type`
* `:start`
* `:end`

When an email is sent the substitutions are replaced with the values specified in the application.
Below is an example of what that might look like.

##### Email Template Example 1
Substitutions:
```
{
    {":name", "Kevin"},
    {":date", "March 31st 2018"}
}
```
Template:
```
Hi :name, today is :date
```
Email sent:
```
Hi Kevin, today is March 31st 2018
```
___

##### Modfy subsitutions

If a template is modified and a new substitution is required,
then a bit of coding will need to be done. At the time of writing, all email templates
are sent from [`Backend/Services/LeaveService.cs`](https://github.com/hahn-kev/gis/blob/2018.03.30.1/Backend/Services/LeaveService.cs)
and you can find the substitutions by searching for `$LEAVE-SUBSTITUTIONS$`

Let's look at modifying a `RequestLeaveApproval` template to include the date that the Leave Request
was created.

Take a look at this [code](https://github.com/hahn-kev/gis/blob/2018.03.30.1/Backend/Services/LeaveService.cs#L183-L205);
lines 183 through 205 should be highlighted. This is the `SendRequestApproval` function.
Here we're making a substitution list and sending it to SendGrid,
specifying that we want to send a `RequestLeaveApproval` template, along with
a few other things, like sender email, destination email, and subject.

Taking a look at the substitutions you can see that the start and end date are coming from the `leaveRequest`,
using the `leaveRequest.StartDate` or `leaveRequest.EndDate` properties.
We want to access the date the Leave Request was created, so let's find where the `StartDate` and 
`EndDate` are defined, then find the name of the created date. If we search for a file called `leaveRequest.cs`
we should find [this](https://github.com/hahn-kev/gis/blob/2018.03.30.1/Backend/Entities/LeaveRequest.cs)
and at [line 23](https://github.com/hahn-kev/gis/blob/2018.03.30.1/Backend/Entities/LeaveRequest.cs#L23)
we can see `StartDate` with `EndDate` right below. If we look down to [line 38](https://github.com/hahn-kev/gis/blob/2018.03.30.1/Backend/Entities/LeaveRequest.cs#L38)
we see `CreatedDate` property. Back in the `SendRequestApproval` function, we can use this property
on the leave request. This might look like the following:
```c#
{":created", leaveRequest.CreatedDate.ToString("MMM d yyyy")}
```

Note the bit on the end that we're calling `.ToString("MMM d yyyy")`. This formats
the date into text. [Here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)
you can read up on the supported formats if those need to be modified.

Next, you'll want to build the application and update the server with the new version.
You should do this before you update the SendGrid template. Otherwise, an email might get sent
without the substitution being replaced.

Finally, you'll want to update the template on SendGrid to use the `:created`
substitution to display the created date.

#### Replace the SendGrid account

If for some reason you've lost access to the SendGrid account,
you'll need to update the config file on the server to use the new account. 

1. Create a new [SendGrid Account](https://sendgrid.com/)
2. Create an API key with access to send emails
3. Update `appsettings.json` `SendGridAPIKey` with API key
4. Create template emails ([example](#email-template-example-1)) for 
   * HR notification
   * Request Leave approval
   * Leave Request notification (to let supervisors know of leave when they don't approve it)
5. Update `appsettings.json` `TemplateSettings` with appropriate template ID's
6. Reboot the GIS application `sudo service kestrel-gis restart`

### Tech used

- Linux server on Google Cloud
- .Net Core 2.0 (Backend)
- [PostgreSQL](https://www.postgresql.org/) (Database)
- [Angular](https://angular.io/) (Frontend framework)
- [JWT](https://jwt.io/) Json web tokens (Authentication)
- [Material](https://material.angular.io/) (Frontend UI)
- [Sentry](https://sentry.io/hahn-kev/GIS/) (Error reporting)
- [SendGrid](https://app.sendgrid.com) (Email)
