﻿using System;
using Warcraft.WMO.RootFile;
using Warcraft.WMO.GroupFile;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Warcraft.WMO.RootFile.Chunks;

namespace Warcraft.WMO
{
    /// <summary>
    /// Container class for a World Model Object (WMO).
    /// This class hosts the root file with metadata definitions, as well as the
    /// group files which contain the actual 3D model data.
    /// </summary>
    public class WMO : IDisposable
    {
        public ModelRoot RootInformation;
        public List<ModelGroup> Groups = new List<ModelGroup>();

        public int GroupCount => (int)RootInformation.Header.GroupCount;

        public WMO(byte[] inData)
        {
            RootInformation = new ModelRoot(inData);

            PostResolveStringReferences();
        }

        private void PostResolveStringReferences() // TODO: Refactor
        {
            foreach (DoodadInstance doodadInstance in RootInformation.DoodadInstances.DoodadInstances)
            {
                string doodadPath = RootInformation.DoodadPaths.GetNameByOffset(doodadInstance.NameOffset);
                doodadInstance.Name = doodadPath;
            }

            foreach (ModelMaterial modelMaterial in RootInformation.Materials.Materials)
            {
                string texturePath0 = RootInformation.Textures.GetTexturePathByOffset(modelMaterial.FirstTextureOffset);
                string texturePath1 = RootInformation.Textures.GetTexturePathByOffset(modelMaterial.SecondTextureOffset);
                string texturePath2 = RootInformation.Textures.GetTexturePathByOffset(modelMaterial.ThirdTextureOffset);

                if (string.IsNullOrEmpty(texturePath0))
                {
                    texturePath0 = "createcrappygreentexture.blp";
                }

                modelMaterial.Texture0 = texturePath0;
                modelMaterial.Texture1 = texturePath1;
                modelMaterial.Texture2 = texturePath2;
            }
        }

        public bool DoesGroupBelongToModel(ModelGroup modelGroup)
        {
            return RootInformation.ContainsGroup(modelGroup);
        }

        /// <summary>
        /// Adds a model group to the model object. The model group must be listed in the root object,
        /// or it won't be accepted by the model.
        /// </summary>
        /// <param name="modelGroupStream">Stream containing the Model group.</param>
        public void AddModelGroup(Stream modelGroupStream)
        {

        }

        /// <summary>
        /// Adds a model group to the model object. The model group must be listed in the root object,
        /// or it won't be accepted by the model.
        /// </summary>
        /// <param name="modelGroup">Model group.</param>
        public void AddModelGroup(ModelGroup modelGroup)
        {
            if (!DoesGroupBelongToModel(modelGroup))
            {
                return;
            }

            modelGroup.Name = ResolveInternalGroupName(modelGroup);
            modelGroup.DescriptiveName = ResolveInternalDescriptiveGroupName(modelGroup);
            Groups.Add(modelGroup);
        }

        /// <summary>
        /// Adds a model group to the model object. The model group must be listed in the root object,
        /// or it won't be accepted by the model.
        /// </summary>
        /// <param name="inData">Byte array containing the model group.</param>
        public void AddModelGroup(byte[] inData)
        {
            ModelGroup group = new ModelGroup(inData);
            AddModelGroup(group);
        }

        public string ResolveInternalGroupName(ModelGroup modelGroup)
        {
            return RootInformation.GetInternalGroupName(modelGroup);
        }

        private string ResolveInternalDescriptiveGroupName(ModelGroup modelGroup)
        {
            return RootInformation.GetInternalDescriptiveGroupName(modelGroup);
        }

        public IEnumerable<string> GetTextures()
        {
            return RootInformation.Textures.Textures.Select(kvp => kvp.Value).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        public ModelMaterial GetMaterial(byte materialID)
        {
            return RootInformation.Materials.Materials[materialID];
        }

        /// <summary>
        /// Gets the materials in this model.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModelMaterial> GetMaterials()
        {
            return RootInformation.Materials.Materials;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="Warcraft.WMO.WMO"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Warcraft.WMO.WMO"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="Warcraft.WMO.WMO"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="Warcraft.WMO.WMO"/> so the garbage
        /// collector can reclaim the memory that the <see cref="Warcraft.WMO.WMO"/> was occupying.</remarks>
        public void Dispose()
        {

        }
    }
}

