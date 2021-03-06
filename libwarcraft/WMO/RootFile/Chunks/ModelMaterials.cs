//
//  ModelMaterials.cs
//
//  Copyright (c) 2018 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.IO;
using Warcraft.Core.Extensions;
using Warcraft.Core.Interfaces;
using Warcraft.Core.Shading;
using Warcraft.Core.Shading.Blending;
using Warcraft.Core.Structures;
using Warcraft.DBC;
using Warcraft.DBC.Definitions;
using Warcraft.DBC.SpecialFields;

namespace Warcraft.WMO.RootFile.Chunks
{
    public class ModelMaterials : IIFFChunk, IBinarySerializable
    {
        /// <summary>
        /// Holds the binary chunk signature.
        /// </summary>
        public const string Signature = "MOMT";

        public readonly List<ModelMaterial> Materials = new List<ModelMaterial>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMaterials"/> class.
        /// </summary>
        public ModelMaterials()
        {
        }

        public ModelMaterials(byte[] inData)
        {
            LoadBinaryData(inData);
        }

        /// <inheritdoc/>
        public void LoadBinaryData(byte[] inData)
        {
            using (MemoryStream ms = new MemoryStream(inData))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int materialCount = inData.Length / ModelMaterial.GetSize();
                    for (int i = 0; i < materialCount; ++i)
                    {
                        Materials.Add(new ModelMaterial(br.ReadBytes(ModelMaterial.GetSize())));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string GetSignature()
        {
            return Signature;
        }

        /// <inheritdoc/>
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    foreach (ModelMaterial modelMaterial in Materials)
                    {
                        bw.Write(modelMaterial.Serialize());
                    }
                }

                return ms.ToArray();
            }
        }
    }

    public class ModelMaterial : IBinarySerializable
    {
        public MaterialFlags Flags;
        public WMOFragmentShaderType Shader;
        public BlendingMode BlendMode;

        public uint FirstTextureOffset;
        public RGBA FirstColour;
        public MaterialFlags FirstFlags;

        public uint SecondTextureOffset;
        public RGBA SecondColour;

        public ForeignKey<uint> GroundType;
        public uint ThirdTextureOffset;
        public RGBA BaseDiffuseColour;
        public MaterialFlags ThirdFlags;

        public uint RuntimeData1;
        public uint RuntimeData2;
        public uint RuntimeData3;
        public uint RuntimeData4;

        /*
            Nonserialized utility fields
        */

        public string Texture0
        {
            get;
            set;
        }

        public string Texture1
        {
            get;
            set;
        }

        public string Texture2
        {
            get;
            set;
        }

        public ModelMaterial(byte[] inData)
        {
            using (MemoryStream ms = new MemoryStream(inData))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Flags = (MaterialFlags) br.ReadUInt32();
                    Shader = (WMOFragmentShaderType) br.ReadUInt32();
                    BlendMode = (BlendingMode) br.ReadUInt32();

                    FirstTextureOffset = br.ReadUInt32();
                    FirstColour = br.ReadRGBA();
                    FirstFlags  = (MaterialFlags)br.ReadUInt32();

                    SecondTextureOffset = br.ReadUInt32();
                    SecondColour = br.ReadRGBA();

                    GroundType = new ForeignKey<uint>(DatabaseName.TerrainType, nameof(DBCRecord.ID), br.ReadUInt32()); // TODO: Implement TerrainTypeRecord
                    ThirdTextureOffset = br.ReadUInt32();
                    BaseDiffuseColour = br.ReadRGBA();
                    ThirdFlags = (MaterialFlags)br.ReadUInt32();

                    RuntimeData1 = br.ReadUInt32();
                    RuntimeData2 = br.ReadUInt32();
                    RuntimeData3 = br.ReadUInt32();
                    RuntimeData4 = br.ReadUInt32();
                }
            }
        }

        public static int GetSize()
        {
            return 64;
        }

        /// <inheritdoc/>
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((uint)Flags);
                    bw.Write((uint)Shader);
                    bw.Write((uint)BlendMode);

                    bw.Write(FirstTextureOffset);
                    bw.WriteRGBA(FirstColour);
                    bw.Write((uint)FirstFlags);

                    bw.Write(SecondTextureOffset);
                    bw.WriteRGBA(SecondColour);

                    bw.Write(GroundType.Key);
                    bw.Write(ThirdTextureOffset);
                    bw.WriteRGBA(BaseDiffuseColour);
                    bw.Write((uint)ThirdFlags);

                    bw.Write(RuntimeData1);
                    bw.Write(RuntimeData2);
                    bw.Write(RuntimeData3);
                    bw.Write(RuntimeData4);
                }

                return ms.ToArray();
            }
        }
    }

    [Flags]
    public enum MaterialFlags : uint
    {
        UnknownPossiblyLightmap = 0x1,
        Unknown2                = 0x2,
        TwoSided                = 0x4,
        Darken                    = 0x8,
        UnshadedDuringNight        = 0x10,
        Unknown3                = 0x20,
        TextureWrappingClamp    = 0x40,
        TextureWrappingRepeat    = 0x80,
        Unknown4                = 0x100

        // Followed by 23 unused flags
    }
}

