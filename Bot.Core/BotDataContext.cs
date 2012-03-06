// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from main on 2012-01-16 17:06:44Z.
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
using System;
using System.ComponentModel;
using System.Data;
using DbLinq.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;


public partial class BotDataContext : DataContext
{
	
	#region Extensibility Method Declarations
			partial void OnCreated();
		#endregion
	
	public BotDataContext(string connectionString) : 
			base(connectionString)
	{
		this.OnCreated();
	}
	
	public BotDataContext(IDbConnection connection) : 
			base(connection)
	{
		this.OnCreated();
	}
	
	public BotDataContext(string connection, MappingSource mappingSource) : 
			base(connection, mappingSource)
	{
		this.OnCreated();
	}
	
	public BotDataContext(IDbConnection connection, MappingSource mappingSource) : 
			base(connection, mappingSource)
	{
		this.OnCreated();
	}

    public Table<UrlLog> UrlLog
    {
        get
        {
            return this.GetTable<UrlLog>();
        }
    }
	
	public Table<User> User
	{
		get
		{
			return this.GetTable <User>();
		}
	}
	
	public Table<UserSetting> UserSetting
	{
		get
		{
			return this.GetTable <UserSetting>();
		}
	}
}

[Table(Name = "main.UrlLog")]
public partial class UrlLog : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
{

    private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");

    private System.DateTime _date;

    private int _id;

    private string _nick;

    private string _title;

    private string _url;

    #region Extensibility Method Declarations
    partial void OnCreated();

    partial void OnDateChanged();

    partial void OnDateChanging(System.DateTime value);

    partial void OnIDChanged();

    partial void OnIDChanging(int value);

    partial void OnNickChanged();

    partial void OnNickChanging(string value);

    partial void OnTitleChanged();

    partial void OnTitleChanging(string value);

    partial void OnUrlChanged();

    partial void OnUrlChanging(string value);
    #endregion

    public UrlLog()
    {
        this.OnCreated();
    }

    [Column(Storage = "_date", Name = "Date", DbType = "DATETIME", AutoSync = AutoSync.Never, CanBeNull = false)]
    [DebuggerNonUserCode()]
    public System.DateTime Date
    {
        get
        {
            return this._date;
        }
        set
        {
            if ((_date != value))
            {
                this.OnDateChanging(value);
                this.SendPropertyChanging();
                this._date = value;
                this.SendPropertyChanged("Date");
                this.OnDateChanged();
            }
        }
    }

    [Column(Storage = "_id", Name = "Id", DbType = "INTEGER", IsPrimaryKey = true, IsDbGenerated = true, AutoSync = AutoSync.Never, CanBeNull = false)]
    [DebuggerNonUserCode()]
    public int ID
    {
        get
        {
            return this._id;
        }
        set
        {
            if ((_id != value))
            {
                this.OnIDChanging(value);
                this.SendPropertyChanging();
                this._id = value;
                this.SendPropertyChanged("ID");
                this.OnIDChanged();
            }
        }
    }

    [Column(Storage = "_nick", Name = "Nick", DbType = "TEXT", AutoSync = AutoSync.Never, CanBeNull = false)]
    [DebuggerNonUserCode()]
    public string Nick
    {
        get
        {
            return this._nick;
        }
        set
        {
            if (((_nick == value) == false))
            {
                this.OnNickChanging(value);
                this.SendPropertyChanging();
                this._nick = value;
                this.SendPropertyChanged("Nick");
                this.OnNickChanged();
            }
        }
    }

    [Column(Storage = "_title", Name = "Title", DbType = "TEXT", AutoSync = AutoSync.Never)]
    [DebuggerNonUserCode()]
    public string Title
    {
        get
        {
            return this._title;
        }
        set
        {
            if (((_title == value) == false))
            {
                this.OnTitleChanging(value);
                this.SendPropertyChanging();
                this._title = value;
                this.SendPropertyChanged("Title");
                this.OnTitleChanged();
            }
        }
    }

    [Column(Storage = "_url", Name = "Url", DbType = "TEXT", AutoSync = AutoSync.Never, CanBeNull = false)]
    [DebuggerNonUserCode()]
    public string Url
    {
        get
        {
            return this._url;
        }
        set
        {
            if (((_url == value) == false))
            {
                this.OnUrlChanging(value);
                this.SendPropertyChanging();
                this._url = value;
                this.SendPropertyChanged("Url");
                this.OnUrlChanged();
            }
        }
    }

    public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected virtual void SendPropertyChanging()
    {
        System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
        if ((h != null))
        {
            h(this, emptyChangingEventArgs);
        }
    }

    protected virtual void SendPropertyChanged(string propertyName)
    {
        System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
        if ((h != null))
        {
            h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}

[Table(Name="main.User")]
public partial class User : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
{
	
	private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
	
	private System.Nullable<int> _id;
	
	private string _lastIdent;
	
	private string _lastNick;
	
	private string _password;
	
	private int _userLevel;
	
	private string _username;
	
	#region Extensibility Method Declarations
			partial void OnCreated();
		
			partial void OnIDChanged();
		
			partial void OnIDChanging(System.Nullable<int> value);
		
			partial void OnLastIdentChanged();
		
			partial void OnLastIdentChanging(string value);
		
			partial void OnLastNickChanged();
		
			partial void OnLastNickChanging(string value);
		
			partial void OnPasswordChanged();
		
			partial void OnPasswordChanging(string value);
		
			partial void OnUserLevelChanged();
		
			partial void OnUserLevelChanging(int value);
		
			partial void OnUsernameChanged();
		
			partial void OnUsernameChanging(string value);
		#endregion
	
	public User()
	{
		this.OnCreated();
	}
	
	[Column(Storage="_id", Name="Id", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public System.Nullable<int> ID
	{
		get
		{
			return this._id;
		}
		set
		{
			if ((_id != value))
			{
				this.OnIDChanging(value);
				this.SendPropertyChanging();
				this._id = value;
				this.SendPropertyChanged("ID");
				this.OnIDChanged();
			}
		}
	}
	
	[Column(Storage="_lastIdent", Name="LastIdent", DbType="TEXT", AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public string LastIdent
	{
		get
		{
			return this._lastIdent;
		}
		set
		{
			if (((_lastIdent == value) == false))
			{
				this.OnLastIdentChanging(value);
				this.SendPropertyChanging();
				this._lastIdent = value;
				this.SendPropertyChanged("LastIdent");
				this.OnLastIdentChanged();
			}
		}
	}
	
	[Column(Storage="_lastNick", Name="LastNick", DbType="TEXT", AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public string LastNick
	{
		get
		{
			return this._lastNick;
		}
		set
		{
			if (((_lastNick == value) == false))
			{
				this.OnLastNickChanging(value);
				this.SendPropertyChanging();
				this._lastNick = value;
				this.SendPropertyChanged("LastNick");
				this.OnLastNickChanged();
			}
		}
	}
	
	[Column(Storage="_password", Name="Password", DbType="TEXT", AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public string Password
	{
		get
		{
			return this._password;
		}
		set
		{
			if (((_password == value) == false))
			{
				this.OnPasswordChanging(value);
				this.SendPropertyChanging();
				this._password = value;
				this.SendPropertyChanged("Password");
				this.OnPasswordChanged();
			}
		}
	}
	
	[Column(Storage="_userLevel", Name="UserLevel", DbType="INTEGER", AutoSync=AutoSync.Never, CanBeNull=false)]
	[DebuggerNonUserCode()]
	public int UserLevel
	{
		get
		{
			return this._userLevel;
		}
		set
		{
			if ((_userLevel != value))
			{
				this.OnUserLevelChanging(value);
				this.SendPropertyChanging();
				this._userLevel = value;
				this.SendPropertyChanged("UserLevel");
				this.OnUserLevelChanged();
			}
		}
	}
	
	[Column(Storage="_username", Name="Username", DbType="TEXT", AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public string Username
	{
		get
		{
			return this._username;
		}
		set
		{
			if (((_username == value) == false))
			{
				this.OnUsernameChanging(value);
				this.SendPropertyChanging();
				this._username = value;
				this.SendPropertyChanged("Username");
				this.OnUsernameChanged();
			}
		}
	}
	
	public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
	
	public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
		if ((h != null))
		{
			h(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(string propertyName)
	{
		System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
		if ((h != null))
		{
			h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
	}
}

[Table(Name="main.UserSetting")]
public partial class UserSetting : System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
{
	
	private static System.ComponentModel.PropertyChangingEventArgs emptyChangingEventArgs = new System.ComponentModel.PropertyChangingEventArgs("");
	
	private int _id;
	
	private string _name;
	
	private System.Nullable<int> _userID;
	
	private string _value;
	
	#region Extensibility Method Declarations
			partial void OnCreated();
		
			partial void OnIDChanged();
		
			partial void OnIDChanging(int value);
		
			partial void OnNameChanged();
		
			partial void OnNameChanging(string value);
		
			partial void OnUserIDChanged();
		
			partial void OnUserIDChanging(System.Nullable<int> value);
		
			partial void OnValueChanged();
		
			partial void OnValueChanging(string value);
		#endregion
	
	public UserSetting()
	{
		this.OnCreated();
	}
	
	[Column(Storage="_id", Name="Id", DbType="INTEGER", IsPrimaryKey=true, IsDbGenerated=true, AutoSync=AutoSync.Never, CanBeNull=false)]
	[DebuggerNonUserCode()]
	public int ID
	{
		get
		{
			return this._id;
		}
		set
		{
			if ((_id != value))
			{
				this.OnIDChanging(value);
				this.SendPropertyChanging();
				this._id = value;
				this.SendPropertyChanged("ID");
				this.OnIDChanged();
			}
		}
	}
	
	[Column(Storage="_name", Name="Name", DbType="TEXT", AutoSync=AutoSync.Never, CanBeNull=false)]
	[DebuggerNonUserCode()]
	public string Name
	{
		get
		{
			return this._name;
		}
		set
		{
			if (((_name == value) == false))
			{
				this.OnNameChanging(value);
				this.SendPropertyChanging();
				this._name = value;
				this.SendPropertyChanged("Name");
				this.OnNameChanged();
			}
		}
	}
	
	[Column(Storage="_userID", Name="UserId", DbType="INTEGER", AutoSync=AutoSync.Never)]
	[DebuggerNonUserCode()]
	public System.Nullable<int> UserID
	{
		get
		{
			return this._userID;
		}
		set
		{
			if ((_userID != value))
			{
				this.OnUserIDChanging(value);
				this.SendPropertyChanging();
				this._userID = value;
				this.SendPropertyChanged("UserID");
				this.OnUserIDChanged();
			}
		}
	}
	
	[Column(Storage="_value", Name="Value", DbType="TEXT", AutoSync=AutoSync.Never, CanBeNull=false)]
	[DebuggerNonUserCode()]
	public string Value
	{
		get
		{
			return this._value;
		}
		set
		{
			if (((_value == value) == false))
			{
				this.OnValueChanging(value);
				this.SendPropertyChanging();
				this._value = value;
				this.SendPropertyChanged("Value");
				this.OnValueChanged();
			}
		}
	}
	
	public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
	
	public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
	
	protected virtual void SendPropertyChanging()
	{
		System.ComponentModel.PropertyChangingEventHandler h = this.PropertyChanging;
		if ((h != null))
		{
			h(this, emptyChangingEventArgs);
		}
	}
	
	protected virtual void SendPropertyChanged(string propertyName)
	{
		System.ComponentModel.PropertyChangedEventHandler h = this.PropertyChanged;
		if ((h != null))
		{
			h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}
	}
}
