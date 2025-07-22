//------------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2025 Tobias Barendt
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.TextCore;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TMPro
{

public class TMPSpriteAtlas : TMP_SpriteAsset
{
	//--------------------------------------------------------------------------
	// Settings
	[SerializeField] public SpriteAtlas spriteAtlas;

#if UNITY_EDITOR

	//--------------------------------------------------------------------------
	// Clear
	//--------------------------------------------------------------------------
	public void Clear()
	{
		version = "";
		
		spriteSheet = null;
		spriteCharacterTable = new();
		spriteGlyphTable = new();
		spriteCharacterLookupTable = new();
		fallbackSpriteAssets = new();
		
		// Clear assets
		foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)))
			if(asset != this)AssetDatabase.RemoveObjectFromAsset(asset);
		material = null;
		spriteSheet = null;
		AssetDatabase.SaveAssetIfDirty(this);

		UpdateLookupTables();
	}
	
	//--------------------------------------------------------------------------
	// UpdateSpriteData
	//--------------------------------------------------------------------------
	public void UpdateSpriteData()
	{
		// Cache old assets
		Dictionary<string, object> oldLookup = new();
		foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this)))
			oldLookup[asset.name] = asset;
		
		// Clear existing data
		Clear();

		// Add all atlas textures
		int atlasIndex = -1;
		var spriteAtlasPath = AssetDatabase.GetAssetPath(spriteAtlas);
		foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(spriteAtlasPath))
		{
			if(asset is Texture2D atlasTexture)
			{
				// Create a new SpriteAsset if needed
				atlasIndex++;
				TMPSpriteAtlas atlasAsset = atlasIndex == 0 ? this : null;
				if(atlasAsset == null)
				{
					string atlasName = spriteAtlas.name + "_" + atlasIndex;
					atlasAsset = oldLookup.ContainsKey(atlasName) ? oldLookup[atlasName] as TMPSpriteAtlas : null;
					if(atlasAsset == null)atlasAsset = ScriptableObject.CreateInstance<TMPSpriteAtlas>();
					atlasAsset.Clear();
					
					
					if(fallbackSpriteAssets == null)fallbackSpriteAssets = new ();
					fallbackSpriteAssets.Add(atlasAsset);
					atlasAsset.name = atlasName;
					AssetDatabase.AddObjectToAsset(atlasAsset, this);
				}

				// Add texture and update material
				atlasAsset.spriteSheet = atlasTexture;
				ShaderUtilities.GetShaderPropertyIDs();
				Shader shader = Shader.Find("TextMeshPro/Sprite");

				string materialName = ((atlasIndex == 0) ? this.name : spriteAtlas.name + "_" + atlasIndex) + "_material";
				atlasAsset.material = oldLookup.ContainsKey(materialName) ? oldLookup[materialName] as Material : null;
				if(atlasAsset.material == null)atlasAsset.material = new Material(shader);
				atlasAsset.material.SetTexture(ShaderUtilities.ID_MainTex, atlasAsset.spriteSheet);
				atlasAsset.material.name = materialName;
				AssetDatabase.AddObjectToAsset(atlasAsset.material, this);

				// Setup atlas
				atlasAsset.version = "1.1.0";
				atlasAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(atlasAsset.name);

				// Add sprites
				AddSprites(atlasAsset);
			}
		}

		// Save
		AssetDatabase.SetMainObject(this, AssetDatabase.GetAssetPath(this));
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssetIfDirty(this);

		// Notify
		if(fallbackSpriteAssets != null)
			foreach(var subAsset in fallbackSpriteAssets)
				TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, subAsset);
		TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, this);
	}

	//--------------------------------------------------------------------------
	// AddSprites
	//--------------------------------------------------------------------------
	private void AddSprites(TMP_SpriteAsset atlas)
	{
		// Get sprite sheet
		Texture2D spriteSheet = atlas.spriteSheet as Texture2D;
		if(spriteSheet == null)return;

		// Get all sprites
		var sprites = new Sprite[spriteAtlas.spriteCount];
		spriteAtlas.GetSprites(sprites);

		// Add sprites to sprite atlas
		uint spriteIndex = 0xFFFFFFFF;
		foreach(var sprite in sprites)
		{
			// Grab Sprite information
			Texture2D spriteTexture;
			Vector2[] spriteUV;
			try
			{
				spriteTexture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite, true);
				spriteUV = UnityEditor.Sprites.SpriteUtility.GetSpriteUVs(sprite, true);
			}
			catch
			{
				Debug.LogError("Failed to get sprite texture or UVs for sprite " + sprite.name + " usually happens when the atlas is not baked yet, bake it and try again.");
				continue;
			}
			// Make sure we got valid information and that the sprite is for the current atlas
			if(spriteTexture == null || spriteUV == null || spriteUV.Length < 2 || spriteTexture != spriteSheet)continue;
			spriteIndex++;

			// Find texture coordinates
			Vector2 min = spriteUV[0];
			Vector2 max = spriteUV[0];
			foreach(var uv in spriteUV)
			{
				min.x = Mathf.Min(min.x, uv.x);
				min.y = Mathf.Min(min.y, uv.y);
				max.x = Mathf.Max(max.x, uv.x);
				max.y = Mathf.Max(max.y, uv.y);
			}
			var UVRect = new Rect(min.x * spriteSheet.width, min.y * spriteSheet.height, (max.x - min.x) * spriteSheet.width, (max.y - min.y) * spriteSheet.height);
			var scale = new Vector2(spriteSheet.width / spriteSheet.width, spriteSheet.height / spriteSheet.height);
			
			// Add glyph
			var glyph = new TMP_SpriteGlyph();
			glyph.index = spriteIndex;
			// OpenGET note: 75% of scale seems to nicely set the baseline of the glyph to the bottom, rather than the centre
			glyph.metrics = new GlyphMetrics(UVRect.width, UVRect.height, 0.0f, UVRect.height * scale.y * 0.75f, UVRect.width * scale.x);
			glyph.glyphRect = new GlyphRect(UVRect);
			glyph.scale = 1.0f;
			glyph.sprite = sprite;
			atlas.spriteGlyphTable.Add(glyph);

			// Add character
			string characterName = sprite.name;
			if(characterName.EndsWith("(Clone)"))
				characterName = characterName.Substring(0, characterName.Length - "(Clone)".Length);
			var character = new TMP_SpriteCharacter(0xFFFE, glyph);
			character.scale = 1.0f;
			character.name = characterName;
			atlas.spriteCharacterTable.Add(character);
		}

		atlas.SortGlyphAndCharacterTables();
		atlas.UpdateLookupTables();
	}
#endif // UNITY_EDITOR

}
}