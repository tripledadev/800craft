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

using System;
using System.Runtime.Serialization;
using fCraft.Drawing;

namespace fCraft.Doors {

    public class Door {

        public String Name { get; set; }

        public String Creator { get; set; }

        public DateTime Created { get; set; }

        public String World { get; set; }

        public Vector3I[] AffectedBlocks { get; set; }

        public DoorRange Range { get; set; }

        public Door() {
            //empty
        }

        public Door( String world, Vector3I[] affectedBlocks, String Name, String Creator ) {
            this.World = world;
            this.AffectedBlocks = affectedBlocks;
            this.Range = Door.CalculateRange( this );
            this.Name = Name;
            this.Creator = Creator;
            this.Created = DateTime.UtcNow;
        }

        public static DoorRange CalculateRange( Door Door ) {
            DoorRange range = new DoorRange( 0, 0, 0, 0, 0, 0 );

            foreach ( Vector3I block in Door.AffectedBlocks ) {
                if ( range.Xmin == 0 ) {
                    range.Xmin = block.X;
                } else {
                    if ( block.X < range.Xmin ) {
                        range.Xmin = block.X;
                    }
                }

                if ( range.Xmax == 0 ) {
                    range.Xmax = block.X;
                } else {
                    if ( block.X > range.Xmax ) {
                        range.Xmax = block.X;
                    }
                }

                if ( range.Ymin == 0 ) {
                    range.Ymin = block.Y;
                } else {
                    if ( block.Y < range.Ymin ) {
                        range.Ymin = block.Y;
                    }
                }

                if ( range.Ymax == 0 ) {
                    range.Ymax = block.Y;
                } else {
                    if ( block.Y > range.Ymax ) {
                        range.Ymax = block.Y;
                    }
                }

                if ( range.Zmin == 0 ) {
                    range.Zmin = block.Z;
                } else {
                    if ( block.Z < range.Zmin ) {
                        range.Zmin = block.Z;
                    }
                }

                if ( range.Zmax == 0 ) {
                    range.Zmax = block.Z;
                } else {
                    if ( block.Z > range.Zmax ) {
                        range.Zmax = block.Z;
                    }
                }
            }

            return range;
        }

        public bool IsInRange( Player player ) {
            if ( ( player.Position.X / 32 ) <= Range.Xmax && ( player.Position.X / 32 ) >= Range.Xmin ) {
                if ( ( player.Position.Y / 32 ) <= Range.Ymax && ( player.Position.Y / 32 ) >= Range.Ymin ) {
                    if ( ( ( player.Position.Z / 32 ) - 1 ) <= Range.Zmax && ( ( player.Position.Z / 32 ) - 1 ) >= Range.Zmin ) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsInRange( Vector3I vector ) {
            if ( vector.X <= Range.Xmax && vector.X >= Range.Xmin ) {
                if ( vector.Y <= Range.Ymax && vector.Y >= Range.Ymin ) {
                    if ( vector.Z <= Range.Zmax && vector.Z >= Range.Zmin ) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static String GenerateName( World world ) {
            if ( world.Map.Doors != null ) {
                if ( world.Map.Doors.Count > 0 ) {
                    bool found = false;

                    while ( !found ) {
                        bool taken = false;

                        foreach ( Door Door in world.Map.Doors ) {
                            if ( Door.Name.Equals( "Door" + world.Map.DoorID ) ) {
                                taken = true;
                                break;
                            }
                        }

                        if ( !taken ) {
                            found = true;
                        } else {
                            world.Map.DoorID++;
                        }
                    }

                    return "Door" + world.Map.DoorID;
                }
            }

            return "Door1";
        }

        public static bool DoesNameExist( World world, String name ) {
            if ( world.Map.Doors != null ) {
                if ( world.Map.Doors.Count > 0 ) {
                    foreach ( Door Door in world.Map.Doors ) {
                        if ( Door.Name.Equals( name ) ) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Remove( Player requester ) {
            NormalBrush brush = new NormalBrush( Block.Air, Block.Air );
            DrawOperation removeOperation = new CuboidDrawOperation( requester );
            removeOperation.AnnounceCompletion = false;
            removeOperation.Brush = brush;
            removeOperation.Context = BlockChangeContext.Unknown;

            this.AffectedBlocks = new Vector3I[2];
            this.AffectedBlocks[0] = new Vector3I( Range.Xmin, Range.Ymin, Range.Zmin );
            this.AffectedBlocks[1] = new Vector3I( Range.Xmax, Range.Ymax, Range.Zmax );

            if ( !removeOperation.Prepare( this.AffectedBlocks ) ) {
                throw new DoorException( "Unable to remove Door." );
            }

            removeOperation.Begin();

            lock ( requester.World.Map.Doors.SyncRoot ) {
                requester.World.Map.Doors.Remove( this );
            }
        }

        public string Serialize() {
            SerializedData data = new SerializedData( this );
            DataContractSerializer serializer = new DataContractSerializer( typeof( SerializedData ) );
            System.IO.MemoryStream s = new System.IO.MemoryStream();
            serializer.WriteObject( s, data );
            return Convert.ToBase64String( s.ToArray() );
        }

        public static Door Deserialize( string name, string sdata, Map map ) {
            byte[] bdata = Convert.FromBase64String( sdata );
            Door Door = new Door();
            DataContractSerializer serializer = new DataContractSerializer( typeof( SerializedData ) );
            System.IO.MemoryStream s = new System.IO.MemoryStream( bdata );
            SerializedData data = ( SerializedData )serializer.ReadObject( s );
            data.UpdateDoor( Door );
            return Door;
        }

        [DataContract]
        private class SerializedData {

            [DataMember]
            public String Name;

            [DataMember]
            public String Creator;

            [DataMember]
            public DateTime Created;

            [DataMember]
            public String World;

            [DataMember]
            public int XMin;

            [DataMember]
            public int XMax;

            [DataMember]
            public int YMin;

            [DataMember]
            public int YMax;

            [DataMember]
            public int ZMin;

            [DataMember]
            public int ZMax;

            public SerializedData( Door Door ) {
                lock ( Door ) {
                    Name = Door.Name;
                    Creator = Door.Creator;
                    Created = Door.Created;
                    World = Door.World;
                    XMin = Door.Range.Xmin;
                    XMax = Door.Range.Xmax;
                    YMin = Door.Range.Ymin;
                    YMax = Door.Range.Ymax;
                    ZMin = Door.Range.Zmin;
                    ZMax = Door.Range.Zmax;
                }
            }

            public void UpdateDoor( Door Door ) {
                Door.Name = Name;
                Door.Creator = Creator;
                Door.Created = Created;
                Door.World = World;
                Door.Range = new DoorRange( XMin, XMax, YMin, YMax, ZMin, ZMax );
            }
        }
    }
}