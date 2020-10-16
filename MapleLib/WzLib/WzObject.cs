/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Drawing;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
    /// <summary>
    /// An abstract class for wz objects
    /// </summary>
    public abstract class WzObject : IDisposable {
        /// <summary>
        /// The name of the object
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// The WzObjectType of the object
        /// </summary>
        public abstract WzObjectType ObjectType { get; }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        public abstract WzObject Parent { get; internal set; }

        /// <summary>
        /// Returns the parent WZ File
        /// </summary>
        public abstract WzFile WzFileParent { get; }

        public WzStringProperty OutLink => this["_outlink"] as WzStringProperty;
        public WzStringProperty InLink => this["_inlink"] as WzStringProperty;

        public WzObject this[string name] {
            get {
                switch (this) {
                    case WzFile f:
                        return f[name];
                    case WzDirectory d:
                        return d[name];
                    case WzImage i:
                        return i[name];
                    case WzImageProperty p:
                        return p[name];
                    default:
                        throw new NotImplementedException(string.Format("cannot index WzObject of type: '{0}'", GetType().ToString()));
                }
            }
        }

        public string FullPath {
            get {
                if (this is WzFile) return ((WzFile) this).WzDirectory.Name;
                string result = this.Name;
                WzObject currObj = this;
                while (currObj.Parent != null) {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }

                return result;
            }
        }

        public virtual object WzValue {
            get { return null; }
        }

        public abstract void Dispose();

        //Credits to BluePoop for the idea of using cast overriding
        //2015 - That is the worst idea ever, removed and replaced with Get* methods

        #region Cast Values

        public virtual int GetInt() {
            throw new NotImplementedException();
        }

        public virtual short GetShort() {
            throw new NotImplementedException();
        }

        public virtual long GetLong() {
            throw new NotImplementedException();
        }

        public virtual float GetFloat() {
            throw new NotImplementedException();
        }

        public virtual double GetDouble() {
            throw new NotImplementedException();
        }

        public virtual string GetString() {
            throw new NotImplementedException();
        }

        public virtual Point GetPoint() {
            throw new NotImplementedException();
        }

        public virtual Bitmap GetBitmap() {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBytes() {
            throw new NotImplementedException();
        }

        #endregion
    }
}