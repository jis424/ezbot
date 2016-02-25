﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LoLLauncher
{
    public enum Region
    {
        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        SG,

        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        MY,

        [ServerValue("prod.lol.garenanow.com")]
        [LoginQueueValue("https://lq.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        SGMY,

        [ServerValue("prodtw.lol.garenanow.com")]
        [LoginQueueValue("https://loginqueuetw.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        TW,

        [ServerValue("prodth.lol.garenanow.com")]
        [LoginQueueValue("https://lqth.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        TH,

        [ServerValue("prodph.lol.garenanow.com")]
        [LoginQueueValue("https://storeph.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        PH,

        [ServerValue("prodvn.lol.garenanow.com")]
        [LoginQueueValue("https://lqvn.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        VN,

        [ServerValue("prod.oc1.lol.riotgames.com")]
        [LoginQueueValue("https://lq.oc1.lol.riotgames.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(false)]
        OCE,

        [ServerValue("prodid.lol.garenanow.com")]
        [LoginQueueValue("https://lqid.lol.garenanow.com/")]
        [LocaleValue("en_US")]
        [UseGarenaValue(true)]
        ID
    }

    public static class RegionInfo
    {
        public static string GetServerValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            ServerValue[] attrs =
               fi.GetCustomAttributes(typeof(ServerValue),
                                       false) as ServerValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static string GetLoginQueueValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            LoginQueueValue[] attrs =
               fi.GetCustomAttributes(typeof(LoginQueueValue),
                                       false) as LoginQueueValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static string GetLocaleValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            LocaleValue[] attrs =
               fi.GetCustomAttributes(typeof(LocaleValue),
                                       false) as LocaleValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }

        public static bool GetUseGarenaValue(Enum value)
        {
            bool output = false;
            Type type = value.GetType();

            FieldInfo fi = type.GetField(value.ToString());
            UseGarenaValue[] attrs =
               fi.GetCustomAttributes(typeof(UseGarenaValue),
                                       false) as UseGarenaValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }
            return output;
        }
    }

    public class ServerValue : System.Attribute
    {
        private string _value;

        public ServerValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class LoginQueueValue : System.Attribute
    {
        private string _value;

        public LoginQueueValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class LocaleValue : System.Attribute
    {
        private string _value;

        public LocaleValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

    public class UseGarenaValue : System.Attribute
    {
        private bool _value;

        public UseGarenaValue(bool value)
        {
            _value = value;
        }

        public bool Value
        {
            get { return _value; }
        }
    }
}