Default to using Dependency Injection for constructors and dependencies.  
When creating new classes default to DI using IServiceProvider unless contructor arguments are required.
using interfaces like IConfiguration and IServiceProvider.
use IConfiguration versus ConfigurationManager
Use WinUI 3 as the UI framework.
Verify all UI elements and xaml is WinUI 3 compliant.
use modern initializations like
Item item = new ();
List<Item> items = [];