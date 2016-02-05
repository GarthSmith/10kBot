// ServerFeaturesState.cs
//
//Ubiety XMPP Library Copyright (C) 2006 - 2015 Dieter Lunn
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

using System.Linq;
using Ubiety.Common;
using Ubiety.Common.Sasl;
using Ubiety.Core;
using Ubiety.Registries;

namespace Ubiety.States
{
    /// <summary>
    ///     The server features state occurs just after connecting.
    /// </summary>
    public class ServerFeaturesState : State
    {
        /// <summary>
        /// </summary>
        /// <param name="data">
        ///     A <see cref="Tag" />
        /// </param>
        public override void Execute(Tag data = null)
        {
            LogQueue.Log("ServerFeaturesState.Execute(" + data + ")");
            Features f;

            var stream = data as Stream;
            if (stream != null)
            {
                var s = stream;
                if (!s.Version.StartsWith("1."))
                {
                    ProtocolState.Events.Error(this, ErrorType.WrongProtocolVersion, ErrorSeverity.Fatal, "Didn't receive expected stream:features tag from 1.x compliant server.");
                    return;
                }
                f = s.Features;
            }
            else
            {
                f = data as Features;
            }

            if (f != null)
            {
                if (f.StartTls != null && ProtocolState.Settings.Ssl)
                {
                    ProtocolState.State = new StartTlsState();
                    var tls = TagRegistry.GetTag<StartTls>("starttls", Namespaces.StartTls);
                    ProtocolState.Socket.Write(tls);
                    return;
                }

                

                if (!ProtocolState.Authenticated)
                {
                    LogQueue.Warn("Setting up SaslProcessor.");
                    ProtocolState.Processor = SaslProcessor.CreateProcessor(f.StartSasl.SupportedTypes, ProtocolState.Settings.AuthenticationTypes);
                    if (ProtocolState.Processor == null)
                    {
                        ProtocolState.State = new DisconnectState();
                        ProtocolState.State.Execute();
                        return;
                    }
                    ProtocolState.Socket.Write(ProtocolState.Processor.Initialize(ProtocolState.Settings.Id, ProtocolState.Settings.Password));

                    ProtocolState.State = new SaslState();
                    ProtocolState.State.Execute(data);
                    return;
                }
                else
                {
                    LogQueue.Warn("Skipping Sasl because ProtocolState.Authenticated");
                }

                // Takes place after authentication according to XEP-0170
                if (!ProtocolState.Compressed && CompressionRegistry.AlgorithmsAvailable && !ProtocolState.Settings.Ssl &&
                    f.Compression != null)
                {
                    // Do we have a stream for any of the compressions supported by the server?
                    foreach (string algorithm in
                        f.Compression.Algorithms.Where(CompressionRegistry.SupportsAlgorithm))
                    {
                        var c = TagRegistry.GetTag<GenericTag>("compress", Namespaces.CompressionProtocol);
                        var m = TagRegistry.GetTag<GenericTag>("method", Namespaces.CompressionProtocol);

                        m.InnerText = ProtocolState.Algorithm = algorithm;
                        c.AddChildTag(m);
                        ProtocolState.Socket.Write(c);
                        ProtocolState.State = new CompressedState();
                        return;
                    }
                }
            }
            /*else
            {
                // f is null
                // HACK: I'm adding this because it seems like livecoding.tv wants it?
                // Probably not good for connecting to other XMPP servers.
                if (!ProtocolState.Authenticated)
                {
                    LogQueue.Warn("Setting up SaslProcessor because f is null.");
                    ProtocolState.Processor = SaslProcessor.CreateProcessor(MechanismType.Plain, MechanismType.Plain);
                    if (ProtocolState.Processor == null)
                    {
                        LogQueue.Warn("Disconnecting because we failed to get a sasl processor..");
                        ProtocolState.State = new DisconnectState();
                        ProtocolState.State.Execute();
                        return;
                    }
                    LogQueue.Warn("Initializing processor and writing.");
                    ProtocolState.Socket.Write(ProtocolState.Processor.Initialize(ProtocolState.Settings.Id, ProtocolState.Settings.Password));

                    ProtocolState.State = new SaslState();
                    ProtocolState.State.Execute(data);
                    return;
                }
            }*/
            
            /*if (!ProtocolState.Authenticated)
            {
                LogQueue.Error("Binding before we authenticate!");
                throw new System.Exception("Not authenticated why binding?");
            }*/


            LogQueue.Warn("Entering BindingState. f is null: " + (f == null) + " and ProtocolState.Authenticated is " + ProtocolState.Authenticated);
            ProtocolState.State = new BindingState();
            ProtocolState.State.Execute();
        }
    }
}