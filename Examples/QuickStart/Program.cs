﻿using FreeSql;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public class MyClass
{

    [Cache2(Key = "Get")]
    public string Get(string key)
    {
        return $"MyClass.Get({key}) value";
    }

    [Cache(Key = "GetAsync")]
    [Cache2(Key = "GetAsync")]
    async public virtual Task<string> GetAsync()
    {
        await Task.Yield();
        return "MyClass.GetAsync value";
    }

    public virtual string Text
    {
        [Cache(Key = "Text")]
        get; 
        set;
    }

    public string T2 {
        get
        {
            return "";
        }
        set
        {
            value = "rgerg";
            Text = value;
        }
    }
}

class Cache2Attribute : FreeSql.DynamicProxyAttribute
{
    [DynamicProxyFromServices]
    public IServiceProvider _service;

    public string Key { get; set; }

    public override Task Before(FreeSql.DynamicProxyBeforeArguments args)
    {
        if (args.Parameters.ContainsKey("key"))
            args.Parameters["key"] = "Newkey";
        return base.Before(args);
    }
    public override Task After(FreeSql.DynamicProxyAfterArguments args)
    {
        return base.After(args);
    }
}


class CacheAttribute : FreeSql.DynamicProxyAttribute
{
    public string Key { get; set; }

    public override Task Before(FreeSql.DynamicProxyBeforeArguments args)
    {
        this.Key = "213234234";
        args.ReturnValue = $"{args.MemberInfo.Name} Before Changed";
        return base.Before(args);
    }
    public override Task After(DynamicProxyAfterArguments args)
    {
        args.ExceptionHandled = true;
        return base.After(args);
    }
}

class Program
{
    static void Main(string[] args)
    {
        FreeSql.DynamicProxy.GetAvailableMeta(typeof(MyClass)); //The first dynamic compilation was slow
        var dt = DateTime.Now;
        var pxy = new MyClass { T2 = "123123" }.ToDynamicProxy();
        Console.WriteLine(pxy.Get("key"));
        Console.WriteLine(pxy.GetAsync().Result);
        pxy.Text = "testSetProp1";
        Console.WriteLine(pxy.Text);

        Console.WriteLine(DateTime.Now.Subtract(dt).TotalMilliseconds + " ms\r\n");

        dt = DateTime.Now;
        pxy = new MyClass().ToDynamicProxy();
        Console.WriteLine(pxy.Get("key1"));
        Console.WriteLine(pxy.GetAsync().Result);
        pxy.Text = "testSetProp2";
        Console.WriteLine(pxy.Text);

        Console.WriteLine(DateTime.Now.Subtract(dt).TotalMilliseconds + " ms\r\n");

        var api = DynamicProxy.Resolve<IUserApi>();
        api.Add(new UserInfo { Id = "001", Remark = "add" });
        Console.WriteLine(JsonConvert.SerializeObject(api.Get<UserInfo>("001")));
    }
}

public interface IUserApi
{
    [HttpGet("api/user")]
    T Get<T>(string id);

    [HttpPost("api/user")]
    void Add<T>(T user);
}
public class UserInfo
{
    public string Id { get; set; }
    public string Remark { get; set; }
}

class HttpGetAttribute : FreeSql.DynamicProxyAttribute
{
    string _url;
    public HttpGetAttribute(string url)
    {
        _url = url;
    }
    public override Task Before(FreeSql.DynamicProxyBeforeArguments args)
    {
        args.ReturnValue = new UserInfo { Id = "ResultId", Remark = $"{args.MemberInfo.Name} HttpGet {_url}" };
        return base.Before(args);
    }
}
class HttpPostAttribute : FreeSql.DynamicProxyAttribute
{
    string _url;
    public HttpPostAttribute(string url)
    {
        _url = url;
    }
    public override Task Before(FreeSql.DynamicProxyBeforeArguments args)
    {
        Console.WriteLine($"{args.MemberInfo.Name} HttpPost {_url} Body {JsonConvert.SerializeObject(args.Parameters)}");
        return base.Before(args);
    }
}