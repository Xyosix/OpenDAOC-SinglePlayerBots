/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;

namespace DOL.GS.Spells
{
	/// <summary>
	/// denotes a class as a spelltype handler for given spell type
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SpellHandlerAttribute : Attribute
	{
		string m_type;

		public SpellHandlerAttribute(string spellType) {
			m_type = spellType;
		}

        public SpellHandlerAttribute(eSpellType spellType)
        {
			m_type = spellType.ToString();
        }

        /// <summary>
        /// Spell type name of the denoted handler
        /// </summary>
        public string SpellType {
			get { return m_type; }
		}
	}
}
