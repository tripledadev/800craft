﻿/*        ----
        Copyright (c) 2011-2013 Jon Baker, Glenn Marien and Lao Tszy <Jonty800@gmail.com>
        All rights reserved.

        Redistribution and use in source and binary forms, with or without
        modification, are permitted provided that the following conditions are met:
         * Redistributions of source code must retain the above copyright
              notice, this list of conditions and the following disclaimer.
            * Redistributions in binary form must reproduce the above copyright
             notice, this list of conditions and the following disclaimer in the
             documentation and/or other materials provided with the distribution.
            * Neither the name of 800Craft or the names of its
             contributors may be used to endorse or promote products derived from this
             software without specific prior written permission.

        THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
        ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
        DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
        ----*/

//Copyright (C) <2011 - 2013> Glenn Mariën (http://project-vanilla.com)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using fCraft.MapConversion;

namespace fCraft {
    public class MessageBlockSerialization : IConverterExtension {
        private static readonly object SaveLoadLock = new object();
        private static List<string> _group = new List<string> { "mblock" };
        public IEnumerable<string> AcceptedGroups { get { return _group; } }

        public int Serialize ( Map map, Stream stream, IMapConverterEx converter ) {
            BinaryWriter writer = new BinaryWriter( stream );
            int count = 0;
            if ( map.MessageBlocks != null ) {
                if ( map.MessageBlocks.Count >= 1 ) {
                    foreach ( MessageBlock MessageBlock in map.MessageBlocks ) {
                        converter.WriteMetadataEntry( _group[0], MessageBlock.Name, MessageBlock.Serialize(), writer );
                        ++count;
                    }
                }
            }
            return count;
        }

        public void Deserialize ( string group, string key, string value, Map map ) {
            try {
                MessageBlock MessageBlock = MessageBlock.Deserialize( key, value, map );
                if ( map.MessageBlocks == null ) map.MessageBlocks = new ArrayList();
                if ( map.MessageBlocks.Count >= 1 ) {
                    if ( map.MessageBlocks.Contains( key ) ) {
                        Logger.Log( LogType.Error, "Map loading warning: duplicate MessageBlock name found: " + key + ", ignored" );
                        return;
                    }
                }
                map.MessageBlocks.Add( MessageBlock );
            } catch ( Exception ex ) {
                Logger.Log( LogType.Error, "MessageBlock.Deserialize: Error deserializing MessageBlock {0}: {1}", key, ex );
            }
        }
    }
}
