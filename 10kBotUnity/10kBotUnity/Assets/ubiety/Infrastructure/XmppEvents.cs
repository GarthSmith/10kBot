﻿// XmppEvents.cs
//
//Ubiety XMPP Library Copyright (C) 2015 Dieter Lunn
//
//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 3 of the License, or (at your option)
//any later version.
//
//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//
//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using Ubiety.Common;

namespace Ubiety.Infrastructure
{
    /// <summary>
    /// </summary>
    public class TagEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="tag"></param>
        public TagEventArgs(Tag tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// </summary>
        public Tag Tag { get; private set; }
    }

    /// <summary>
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="severity"></param>
        public ErrorEventArgs(string message, ErrorType type, ErrorSeverity severity)
        {
            Message = message;
            Type = type;
            Severity = severity;
        }

        /// <value>
        ///     The default error message.
        /// </value>
        public string Message { get; private set; }

        /// <value>
        ///     The type of error that is being returned.
        /// </value>
        public ErrorType Type { get; private set; }

        /// <summary>
        /// </summary>
        public ErrorSeverity Severity { get; private set; }
    }

    /// <summary>
    /// </summary>
    public class XmppEvents
    {
        #region Internal Connect

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs> OnConnect;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Connect(object sender, EventArgs args = default (EventArgs))
        {
            if (OnConnect != null) OnConnect.Invoke(sender, args);

            // OnConnect?.Invoke(sender, args);
        }

        #endregion

        #region Internal Disconnect

        /// <summary>
        /// </summary>
        public event EventHandler<EventArgs> OnDisconnect;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Disconnect(object sender, EventArgs args = default (EventArgs))
        {
            if (OnDisconnect != null) OnDisconnect.Invoke(sender, args);
            // OnDisconnect?.Invoke(sender, args);
        }

        #endregion

        #region Internal Send

        /// <summary>
        /// </summary>
        public event EventHandler<TagEventArgs> OnSend;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Send(object sender, TagEventArgs args)
        {
            if (OnSend != null) OnSend.Invoke(sender, args);
            // OnSend?.Invoke(sender, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tag"></param>
        public void Send(object sender, Tag tag)
        {
            Send(sender, new TagEventArgs(tag));
        }

        #endregion

        #region New Tag

        /// <summary>
        /// </summary>
        public event EventHandler<TagEventArgs> OnNewTag;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void NewTag(object sender, TagEventArgs args)
        {
            if (OnNewTag != null)
            {
                LogQueue.Log("XmppEvents is announcing we have a new tag: " + args.Tag);
                OnNewTag.Invoke(sender, args);
            }
            else
                LogQueue.Warn("We received a response from the server but no one was listening. " + args.Tag.Value);
            // OnNewTag?.Invoke(sender, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tag"></param>
        public void NewTag(object sender, Tag tag)
        {
            // LogQueue.Log("XmppEvents is wrapping a tag up in a TagEventArgs.");
            NewTag(sender, new TagEventArgs(tag));
        }

        #endregion

        /// <summary>
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Error(object sender, ErrorEventArgs args)
        {
            if (OnError != null) OnError.Invoke(sender, args);
            // OnError?.Invoke(sender, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="type"></param>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        public void Error(object sender, ErrorType type, ErrorSeverity severity, String message)
        {
            Error(sender, new ErrorEventArgs(message, type, severity));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="type"></param>
        /// <param name="severity"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public void Error(object sender, ErrorType type, ErrorSeverity severity, String message, params object[] parameters)
        {
            Error(sender, type, severity, String.Format(message, parameters));
        }
    }
}