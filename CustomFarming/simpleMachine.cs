﻿using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using StardewValley;
using StardewValley.Tools;
using StardewValley.Objects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;



namespace CustomFarming
{
    class simpleMachine : StardewValley.Object , ISaveObject, ICustomFarmingObject
    {

        public Texture2D tilesheet;
        public int tilesheetindex;
        public int tileindex;
        public Vector2 tileSize;
        public string description;
        public int productionTime;
        public Item produce;
        public bool usePrefix;
        public string categoryName;
        public string produceName;
        public new Item heldObject;
        List<StardewValley.Object> materials;
        public Microsoft.Xna.Framework.Rectangle sourceRectangle;
        public int tilesheetWidth;
        public int animationFrames;
        public int animationSpeed;
        public int animationFrame;
        public int workAnimationOffset;
        public int workAnimationFrames;
        public bool animateWork;
        public bool animate;
        public bool isWorking;
        public string prefix;
        public StardewValley.Object lastDropIn;
        public string modFolder;
        public dynamic loadJson;
        public int requiredStack;
        public int starterMaterial;
        public int starterMaterialStack;
        public JArray specialProduce;
        public bool isSpecial;
        public bool specialPrefix;
        public string filename;
        public bool useColor;
        public int mil;
        public bool displayItem;
        public int displayItemX;
        public int displayItemY;
        public bool takeAll;
        double displayItemZoom;
        int produceID;
        int produceIndex;

        private bool inStorage;
        private GameLocation environment;

        public bool InStorage
        {
            get
            {
                return this.inStorage;
            }

            set
            {
                this.inStorage = value;
            }
        }

        public GameLocation Environment
        {
            get
            {
                return this.environment;
            }

            set
            {
                this.environment = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return this.tileLocation;
            }

            set
            {
                this.tileLocation = value;
            }
        }

        public Texture2D Texture
        {
            get
            {
                return tilesheet;
            }

        }

        public Microsoft.Xna.Framework.Rectangle SourceRectangle
        {
            get
            {
                return this.sourceRectangle;
            }

        }

        public StardewValley.Object getReplacement()
        {
            StardewValley.Object replacement = new Chest(true);
            return replacement;
        }


        public dynamic getAdditionalSaveData()
        {
            int lastDropInPSI = 0;
            if (this.lastDropIn != null)
            {
                lastDropInPSI = this.lastDropIn.parentSheetIndex;
            }
            dynamic additionalData = new { FileName = this.filename, ModFolder = this.modFolder, LastDropIn = lastDropInPSI, Ready = this.readyForHarvest, Minutes = this.minutesUntilReady, Working = this.isWorking };
            return additionalData;
        }

        public void rebuildFromSave(dynamic additionalSaveData)
        {
            string m = (string)additionalSaveData.ModFolder;
            string f = (string)additionalSaveData.FileName;
            this.build(m, f);
            if (additionalSaveData.LastDropIn != null && additionalSaveData.Minutes != null && additionalSaveData.Working != null && additionalSaveData.Ready != null)
            {
                int lastDropInPSI = (int)additionalSaveData.LastDropIn;
                if (lastDropInPSI != 0)
                { 
                this.lastDropIn = new StardewValley.Object(lastDropInPSI, this.requiredStack);
                this.prefix = this.lastDropIn.name;
                }

                if ((bool)additionalSaveData.Working || (bool) additionalSaveData.Ready)
                {
                    startWorking();
                }

                this.minutesUntilReady = (int)additionalSaveData.Minutes - 480;
                if (this.minutesUntilReady < 1)
                {
                    this.minutesUntilReady = 0;
                }

            }

            
        }

        public void build(string modFolder, string filename)
        {
            SaveHandler.register(this);
            this.tileLocation = Vector2.Zero;
            this.modFolder = modFolder;
            this.filename = filename;
            string path = Path.Combine(modFolder, filename);
            this.loadJson = JObject.Parse(File.ReadAllText(path));
            
            this.takeAll = false;
            this.tileSize = new Vector2(16, 32);
            this.category = -8;
            this.mil = 0;
            this.bigCraftable = true;
            this.isRecipe = false;
            this.animationFrame = 0;
            this.animationFrames = 1;
            this.animationSpeed = 300;

            this.displayItem = false;
            this.displayItemX = 0;
            this.displayItemY = 0;
            this.displayItemZoom = 1.0;

            if (loadJson.displayItem != null && loadJson.displayItemX != null && loadJson.displayItemY != null && loadJson.displayItemZoom != null)
            {
           
                this.displayItem = (bool)loadJson.displayItem;
                this.displayItemX = (int)loadJson.displayItemX;
                this.displayItemY = (int)loadJson.displayItemY;
                this.displayItemZoom = (double)loadJson.displayItemZoom;

            }

                this.isWorking = false;
            this.parentSheetIndex = -1;
            this.readyForHarvest = false;
            this.prefix = "";
            this.type = "Crafting";
            this.isSpecial = false;

            string tilesheetFile = Path.Combine(modFolder, (string)loadJson.Tilesheet);
            Image tilesheetImage = Image.FromFile(tilesheetFile);
            this.tilesheet = Bitmap2Texture(new Bitmap(tilesheetImage));

            this.tilesheetWidth = (int)(tilesheet.Width / tileSize.X);

            this.boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
            this.workAnimationFrames = 0;
            if (loadJson.WorkAnimationFrames != null)
            {
                this.workAnimationFrames = (int)loadJson.WorkAnimationFrames;
            }

            this.workAnimationOffset = (this.workAnimationFrames > 0) ? 1 : 0;

            this.animate = (this.animationFrames > 1) ? true : false;
            this.animateWork = (this.workAnimationFrames > 1) ? true : false;

            this.tilesheetindex = (int)loadJson.TileIndex;
            this.tileindex = tilesheetindex;

            this.sourceRectangle = new Microsoft.Xna.Framework.Rectangle((this.tileindex % tilesheetWidth) * (int)tileSize.X, (int)Math.Floor(this.tilesheetindex / this.tileSize.Y), (int)this.tileSize.X, (int)this.tileSize.Y);

            this.name = (string)loadJson.Name;
            this.categoryName = (string)loadJson.CategoryName;

            this.description = (string)loadJson.Description;
           
            if (loadJson.Produce.Tilesheet != null && (string)loadJson.Produce.Tilesheet != "")
            {
                string produceTilesheetFile = Path.Combine(modFolder, (string)loadJson.Produce.Tilesheet);
                this.produceID = (int)loadJson.Produce.ProduceID;
                this.produceIndex = (int)loadJson.Produce.TileIndex;

                this.produce = new customNamedObject(this.produceID, Path.Combine(modFolder, (string)loadJson.Produce.Tilesheet), this.produceIndex, (int)loadJson.Produce.Stack, (string)loadJson.Produce.Name, (string)loadJson.Produce.Description, Microsoft.Xna.Framework.Color.White);

            this.produceName = (string)loadJson.Produce.Name;
            }

            this.usePrefix = (bool)loadJson.Produce.usePrefix;

            this.useColor = (bool)loadJson.Produce.useColor;

            this.productionTime = (int)loadJson.Produce.ProductionTime;
            this.requiredStack = (int)loadJson.RequieredStack;

            this.specialProduce = (JArray)loadJson.SpecialProduce;

            this.starterMaterial = (int)loadJson.StarterMaterial;
            this.starterMaterialStack = (int)loadJson.StarterMaterialStack;

            JArray loadMaterials = (JArray)loadJson.Materials;
            this.materials = new List<StardewValley.Object>();
            

            if (loadMaterials.Count > 0)
            {
                foreach (var material in loadMaterials)
                {
                    int m = (int)material;

                    if (m == -999)
                    {
                        this.takeAll = true;
                    }

                    if (m > 0)
                    {
                        materials.Add(new StardewValley.Object(m, 1));
                    }
                    else
                    {
                        foreach (int keyI in Game1.objectInformation.Keys)
                        {

                            string[] splitData = Game1.objectInformation[keyI].Split('/');

                            string[] splitCategory = splitData[3].Split(' ');
                            if (splitCategory.Length > 1)
                            {
                                int categoryInt = 0;
                                int.TryParse(splitCategory[1], out categoryInt);

                                if (categoryInt == m)
                                {
                                    materials.Add(new StardewValley.Object(keyI, 1));
                                }
                            }
                        }
                    }
                }
            }
        }

        public simpleMachine()
        {

           
        }

        public simpleMachine(string modFolder, string filename)
        {
            this.build(modFolder, filename);

        }

        public Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(Game1.graphics.GraphicsDevice, s);

            return tx;

        }

        public override bool isPassable()
        {
            return false;
        }

        public override Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
        {
            this.boundingBox.X = (int)tileLocation.X * Game1.tileSize;
            this.boundingBox.Y = (int)tileLocation.Y * Game1.tileSize;
           
            return this.boundingBox;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if(l.isFarm && !l.objects.ContainsKey(tile))
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public override bool canBePlacedInWater()
        {
            return false;
        }

        
        public new Vector2 getScale()
        {
            if (this.workAnimationFrames > 1)
            {
                return Vector2.Zero;
            }
            else
            {
 
                if (this.heldObject == null && this.minutesUntilReady <= 0 || this.readyForHarvest)
                    return Vector2.Zero;
               
                this.scale.X -= 0.1f;
                this.scale.Y += 0.1f;
                if ((double)this.scale.X <= 0.0)
                { 
                    this.scale.X = 10f;
                }
                if ((double)this.scale.Y >= 10.0)
                {
                    this.scale.Y = 0.0f;
                }
                    
                return new Vector2(Math.Abs(this.scale.X - 5f), Math.Abs(this.scale.Y - 5f)); ;
            }
           
        }
        

        public bool deliverProduce(Farmer who)
        {
            if (who.IsMainPlayer && !who.addItemToInventoryBool((Item)this.heldObject, false))
            {
                Game1.showRedMessage("Inventory Full");
                return false;
            }

            this.readyForHarvest = false;
            this.lastDropIn = null;
            this.heldObject = null;
            this.isSpecial = false;

            if (this.materials.Count == 0)
            {
                this.startWorking();
            }

            Game1.playSound("coin");
            return true;

        }


        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {

            if (this.description.Contains("arecrow") && this.produce == null)
            {
                if (justCheckingForActivity) { return true; }
                this.shakeTimer = 100;
                
                if (this.specialVariable == 0)
                {
                    Game1.drawObjectDialogue("I haven't encountered any crows yet.");
                }
                else
                {
                    Game1.drawObjectDialogue("I've scared off " + (object)this.specialVariable + " crow" + (this.specialVariable == 1 ? "." : "s."));
                }
                    
                return true;
            }

            if (this.heldObject == null)
            {
              
                bool check = (this.materials.FindIndex(x => (who.ActiveObject is StardewValley.Object) && x.parentSheetIndex == who.ActiveObject.parentSheetIndex) == -1) ? false : true;
                
                return check;
            }

            if (!this.readyForHarvest)
            {
                return false;
            }

            if (justCheckingForActivity)
            {
                return true;
            }

            this.deliverProduce(who);
            return true;
        }
  
        public override void DayUpdate(GameLocation location)
        {


        }

        public override void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {

            Vector2 vector2 = this.getScale() * (float)Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize * 2) + (double)vector2.Y / 2.0));

            
            spriteBatch.Draw(this.tilesheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (double) x * 9.99999974737875E-06));

            if (this.displayItem && this.lastDropIn != null)
            {
            
                Microsoft.Xna.Framework.Rectangle displayDestinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0)+ this.displayItemX + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + this.displayItemY + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)( this.displayItemZoom * ( (int)((double)Game1.tileSize + (double)vector2.X) )), (int) (this.displayItemZoom * ( (int)((double)(Game1.tileSize) + (double)vector2.Y / 2.0))));

                spriteBatch.Draw(Game1.objectSpriteSheet, displayDestinationRectangle, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.lastDropIn.parentSheetIndex, 16, 16)), Microsoft.Xna.Framework.Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y+1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (double) (x+1) * 9.99999974737875E-06));

            }

            if (this.readyForHarvest && this.heldObject != null) {
                    customNamedObject cno = (this.heldObject as customNamedObject);
                    float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize - 8), (float)(y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Microsoft.Xna.Framework.Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((y + 1) * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + (double)this.tileLocation.X / 10000.0 + (this.parentSheetIndex == 105 ? 0.00150000001303852 : 0.0)));
                    spriteBatch.Draw(cno.tilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2), (float)(y * Game1.tileSize - Game1.tileSize - Game1.tileSize / 8) + num)), new Microsoft.Xna.Framework.Rectangle?(cno.sourceRectangle), cno.color * 0.75f, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom, SpriteEffects.None, (float)((double)((y + 1) * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + (double)this.tileLocation.X / 10000.0 + 0.0));
                }

            SaveHandler.draw(this, new Vector2(x,y));
        }


        public override void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {

            this.draw(spriteBatch, xNonTile, yNonTile, alpha);

        }
        
        public override void drawInMenu(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(this.tilesheet, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White * transparency, 0.0f, new Vector2(tileSize.X/2, tileSize.Y/2), (float)Game1.pixelZoom * ((double)scaleSize < 0.2 ? scaleSize : scaleSize / 2.00f), SpriteEffects.None, layerDepth);
            SaveHandler.drawInMenu(this);
        }


        public override void drawWhenHeld(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(this.tilesheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        }


        public override Microsoft.Xna.Framework.Color getCategoryColor()
        {
            return Microsoft.Xna.Framework.Color.Magenta;
        }

        public override string getCategoryName()
        {

            return this.categoryName;

        }

        public override string getDescription()
        {

            return this.description;

        }

        public override bool isActionable(Farmer who)
        {
            return this.checkForAction(who, true);
        }

        public override bool isPlaceable()
        {
            return true;
        }
       
        public override bool minutesElapsed(int minutes, GameLocation environment)
        {
            this.minutesUntilReady = this.minutesUntilReady - minutes;

            if (this.minutesUntilReady <= 0 && this.heldObject != null && !this.readyForHarvest && Game1.currentLocation.Equals((object)environment))
            {
                    Game1.playSound("dwop");
                    this.readyForHarvest = true;
                    this.minutesUntilReady = 0;
                    this.isWorking = false;
            }

            if (!this.readyForHarvest && this.materials.Count == 0 && !this.isWorking)
            {

                this.startWorking();
            }

                return false;
        }
       
        public override bool performDropDownAction(Farmer who)
        {
          if(this.materials.Count == 0)
            {
                startWorking();
            }

            return false;
        }


        public override bool performObjectDropInAction(StardewValley.Object dropIn, bool probe, Farmer who)
        {

            if (dropIn != null && dropIn.bigCraftable || this.heldObject != null)
            {
                return false;
            }
            
            if ((dropIn == null || (dropIn != null & this.materials.FindIndex(x => x.parentSheetIndex == dropIn.parentSheetIndex) == -1)) && !this.takeAll)
            {
                return false;
            }

            if (dropIn.stack < this.requiredStack)
            {
                if (!probe) { 
                Game1.showRedMessage("Requieres " + this.requiredStack + " " + dropIn.name);
                }
                return false;
            }

            if (this.starterMaterial != 0 && who.IsMainPlayer && who.getTallyOfObject(this.starterMaterial, false) <= 0)
            {
                if (!probe)
                {
                    Game1.showRedMessage("Requieres " + this.starterMaterialStack + " " + new StardewValley.Object(this.starterMaterial, 1).name);
                }
                return false;
            }

            int sum = this.starterMaterialStack + this.requiredStack;

            if (dropIn.parentSheetIndex == this.starterMaterial && dropIn.Stack < sum)
            {
                if (!probe)
                { 
                    Game1.showRedMessage("Requieres " + sum + " " + new StardewValley.Object(this.starterMaterial, 1).name);
                }
                return false;
            }


            if (!probe)
            {
                this.lastDropIn = dropIn;
                this.prefix = dropIn.name;
              

                if (this.starterMaterial != 0 && dropIn.parentSheetIndex == this.starterMaterial)
                {
           
                    if (dropIn.Stack <= (sum - 1))
                    {
                        who.removeItemFromInventory((Item)dropIn);
                    }
                    else
                    {
                        dropIn.Stack -= (sum - 1);
                    }
                    startWorking();
                    Game1.playSound("Ship");
                    return true;
                }
                else
                {
                    if (dropIn.Stack <= (this.requiredStack-1))
                    {
                        who.removeItemFromInventory((Item)dropIn);
                    }
                    else
                    {
                        dropIn.Stack -= (this.requiredStack-1);
                    }
                }


                if (this.starterMaterial != 0 && dropIn.parentSheetIndex != this.starterMaterial)
                {
                    int count = 0;
                    while (count < this.starterMaterialStack)
                    {
                        int index = who.items.FindIndex(x => x != null && x is StardewValley.Object && x.parentSheetIndex == this.starterMaterial);
                        if(index == -1)
                        {
                            break;
                        }

                        if (who.items[index].Stack <= 1)
                        {
                            who.Items[index] = (Item)null;
                        }
                        else
                        { 
                        who.items[index].Stack--;
                        }
                        count++;
                    }

                }

                startWorking();
                Game1.playSound("Ship");
                return true;
            }

            return true;
            
        }

        public override Item getOne()
        {
            return new simpleMachine(modFolder, filename);
        }

        public override bool performToolAction(Tool t)
        {

            if (t == null || !t.isHeavyHitter() || t is MeleeWeapon || !(t is Pickaxe))
            {
                return false;
            }
          
            if (this.heldObject != null)
            {
                return false;
            }

            if (this.heldObject == null)
            {
                Game1.playSound("hammer");
                Game1.currentLocation.objects.Remove(this.tileLocation);
                Game1.createItemDebris(this, this.tileLocation * (float)Game1.tileSize, -1, (GameLocation)null);
                return false;
            }
                
            this.heldObject = (StardewValley.Object)null;
            this.readyForHarvest = false;
            this.minutesUntilReady = -1;
            return false;

        }

        public void checkForSpecialProduce()
        {


            this.specialPrefix = false;
            this.isSpecial = false;

            if (this.specialProduce != null && this.specialProduce.Count > 0)
            {

                foreach (dynamic p in this.specialProduce)
                {
                    int m = (int)p.Material;
                    if (this.lastDropIn.parentSheetIndex == m || this.lastDropIn.category == m)
                    {
                        this.heldObject.Name = (string)p.Name;
                        this.specialPrefix = (bool)p.usePrefix;
                        if (p.TileIndex != null)
                        {
                            (this.heldObject as customNamedObject).tilesheetindex = p.TileIndex;
                            (this.heldObject as customNamedObject).rebuildSoureceRect();
                        }

                        if (p.ProduceID != null)
                        {
                            (this.heldObject as customNamedObject).parentSheetIndex = p.ProduceID;
                            this.heldObject = new customNamedObject((this.heldObject as customNamedObject).parentSheetIndex, (this.heldObject as customNamedObject).tilesheetpath, (this.heldObject as customNamedObject).tilesheetindex, (this.heldObject as customNamedObject).Stack, (this.heldObject as customNamedObject).name, (this.heldObject as customNamedObject).description, (this.heldObject as customNamedObject).color);

                        }

                        this.isSpecial = true;
                        break;
                    }

                }
            }

        

        }

        public void buildProduce()
        {

            this.heldObject = produce.getOne();
            this.heldObject.Stack = produce.Stack;
            this.heldObject.Name = this.produceName;

            if (this.materials.Count == 0)
            {
                return;
            }

            checkForSpecialProduce();


            if ((this.usePrefix && !this.isSpecial) || (this.isSpecial && this.specialPrefix))
            {
                this.heldObject.Name = this.prefix + " " + this.heldObject.Name;
            }
        }

        public void startWorking()
        {

            if (this.produce == null) { this.isWorking = true; return; }

            buildProduce();

            this.minutesUntilReady = this.productionTime;
            this.isWorking = true;

            if (this.materials.Count == 0)
            {
                return;
            }


            if (this.heldObject != null && this.lastDropIn != null)
            {             
                int x = (this.lastDropIn.parentSheetIndex % (Game1.objectSpriteSheet.Width / 16)) * 16 + 7;
                int y = (int) Math.Floor((decimal)(this.lastDropIn.parentSheetIndex / (Game1.objectSpriteSheet.Width/16))) * 16 + 7;

                if (this.useColor)
                {
                    (this.heldObject as customNamedObject).pickColorFromSprite(Game1.objectSpriteSheet, x, y);
                }

            }

            
        }
      
        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));

            if (location.objects.ContainsKey(index1))
            {
                return false;
            }

            StardewValley.Object placeObject = (StardewValley.Object)this.getOne();
            placeObject.tileLocation = index1;

            location.objects[index1] = placeObject;

           

            if (this.materials.Count == 0)
            {
                (placeObject as simpleMachine).startWorking();
            }
             
            
            return true;
        }
        

        public override void updateWhenCurrentLocation(GameTime time)
        {

            if (this.shakeTimer > 0)
            {
                this.shakeTimer = this.shakeTimer - time.ElapsedGameTime.Milliseconds;
                if (this.shakeTimer <= 0)
                    this.health = 10;
            }

       
            if (!(time.TotalGameTime.TotalMilliseconds % this.animationSpeed <= 10.0))
            {
                return;
            }


            if (this.isWorking && this.animateWork)
            {
                this.animationFrame++;
                if (this.animationFrame >= this.workAnimationFrames)
                {
                    this.animationFrame = 0;
                }

            }
            else if (!this.isWorking || !this.animateWork)
            {
                this.animationFrame = 0;
                this.tileindex = this.tilesheetindex;
            }
            
            if (!this.isWorking)
            {
                this.tileindex = this.tilesheetindex + this.animationFrame;
            }
            else
            {
                this.tileindex = this.tilesheetindex + this.workAnimationOffset + this.animationFrame;
            }

            this.sourceRectangle = new Microsoft.Xna.Framework.Rectangle((this.tileindex % tilesheetWidth) * (int)tileSize.X, (int)Math.Floor(this.tilesheetindex / this.tileSize.Y), (int)this.tileSize.X, (int)this.tileSize.Y);

        }

       
    }
}