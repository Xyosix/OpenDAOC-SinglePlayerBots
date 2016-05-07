﻿/*
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
using System.Linq;
using System.Reflection;

using DOL.GS;
using DOL.Database;
using DOL.Database.Connection;
using DOL.Database.Attributes;

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Description of RegisterTableTests.
	/// </summary>
	[TestFixture]
	public class RegisterTableTests
	{
		public RegisterTableTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		protected virtual SQLObjectDatabase GetDatabaseV2 { get { return (SQLObjectDatabase)ObjectDatabase.GetObjectDatabase(ConnectionType.DATABASE_SQLITE, DatabaseSetUp.ConnectionString); } }
		
		/// <summary>
		/// Test to Register all Assemblies Tables
		/// </summary>
		[Test]
		public void TestAllAvailableTables()
		{
			foreach (Assembly assembly in new [] { typeof(GameServer).Assembly, typeof(DataObject).Assembly })
			{
				// Walk through each type in the assembly
				foreach (Type type in assembly.GetTypes())
				{
					object[] attrib = type.GetCustomAttributes(typeof(DataTable), true);
					if (attrib.Length > 0)
					{
						Assert.DoesNotThrow( () => {
						                    	var dth = new DataTableHandler(type);
						                    	Database.CheckOrCreateTableImpl(dth);
						                    }, "Registering All Projects Tables should not throw Exceptions... (Failed on Type {0})", type.FullName);
						
						Database.RegisterDataObject(type);
						var selectall = typeof(IObjectDatabase).GetMethod("SelectAllObjects", new Type[] { }).MakeGenericMethod(type);
						object objs = null;
						Assert.DoesNotThrow( () => { objs = selectall.Invoke(Database, new object[] { }); }, "Registered tables should not Throw Exception on Select All... (Failed on Type {0})", type);
						Assert.IsNotNull(objs);
					}
				}
			}
		}
		
		/// <summary>
		/// Wrong Data Object shouldn't be registered
		/// </summary>
		[Test]
		public void TestWrongDataObject()
		{
			Assert.Throws(typeof(ArgumentException), () => {
			              	var dth = new DataTableHandler(typeof(AttributesUtils));
			              	Database.CheckOrCreateTableImpl(dth);
			              }, "Registering a wrong DataObject should throw Argument Exception");
		}
		
		/// <summary>
		/// Multi Index DataObject
		/// </summary>
		[Test]
		public void TestMultiIndexesDataObject()
		{
			var dth = new DataTableHandler(typeof(TestTableWithMultiIndexes));
			Assert.DoesNotThrow(() => Database.CheckOrCreateTableImpl(dth), "Registering Test Table with Overlapping Indexes should not Throw exceptions.");
		}
		
		/// <summary>
		/// Test Table Migration from no PK to PK Auto Inc
		/// </summary>
		[Test]
		public void TestTableMigrationFromNoPrimaryKeyToAutoInc()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableWithNoPrimaryV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			Database.RegisterDataObject(typeof(TestTableWithNoPrimaryV1));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableWithNoPrimaryV1>());
			
			Assert.IsEmpty(Database.SelectAllObjects<TestTableWithNoPrimaryV1>(), "Test Table TestTableWithNoPrimaryV1 should be empty to begin this tests.");
			
			var objs = new [] { "TestObj1", "TestObj2", "TestObj3" }.Select(ent => new TestTableWithNoPrimaryV1 { Value = ent }).ToArray();
			
			Database.AddObject(objs);
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.Value), Database.SelectAllObjects<TestTableWithNoPrimaryV1>().Select(obj => obj.Value), "Test Table TestTableWithNoPrimaryV1 Entries should be available for this test to run.");
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableWithNoPrimaryV2));
			
			var newObjs = DatabaseV2.SelectAllObjects<TestTableWithNoPrimaryV2>().ToArray();
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.Value), newObjs.Select(obj => obj.Value), "Test Table Migration to TestTableWithNoPrimaryV2 should retrieve similar values that created ones...");
			
			Assert.IsTrue(newObjs.All(obj => obj.PrimaryKey != 0), "Test Table Migration to TestTableWithNoPrimaryV2 should have created and populated Primary Key Auto Increment.");
			
			// Trigger Another Migration
			DatabaseV2 = GetDatabaseV2;
			DatabaseV2.RegisterDataObject(typeof(TestTableWithNoPrimaryV3));
			
			var newerObjs = DatabaseV2.SelectAllObjects<TestTableWithNoPrimaryV3>().ToArray();
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.Value), newerObjs.Select(obj => obj.Value), "Test Table Migration to TestTableWithNoPrimaryV3 should retrieve similar values that created ones...");
			
			Assert.IsTrue(newerObjs.All(obj => obj.PrimaryKey2 != 0), "Test Table Migration to TestTableWithNoPrimaryV3 should have created and populated Primary Key Auto Increment.");

		}
		
		/// <summary>
		/// Test Table Migration with Different Types Change
		/// </summary>
		[Test]
		public void TestTableMigrationWithDifferentTypes()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableDifferentTypesV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			Database.RegisterDataObject(typeof(TestTableDifferentTypesV1));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableDifferentTypesV1>());
			
			Assert.IsEmpty(Database.SelectAllObjects<TestTableDifferentTypesV1>(), "Test Table TestTableDifferentTypesV1 should be empty to begin this tests.");
			
			var datenow = DateTime.UtcNow;
			var now = new DateTime(datenow.Year, datenow.Month, datenow.Day, datenow.Hour, datenow.Minute, datenow.Second);
			var objs = new [] { "TestObj1", "TestObj2", "TestObj3" }.Select((ent, i) => new TestTableDifferentTypesV1 { StringValue = ent, IntValue = i, DateValue = now }).ToArray();
			
			Database.AddObject(objs);
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.StringValue), Database.SelectAllObjects<TestTableDifferentTypesV1>().Select(obj => obj.StringValue), "Test Table TestTableDifferentTypesV1 Entries should be available for this test to run.");
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableDifferentTypesV2));
			
			var newObjs = DatabaseV2.SelectAllObjects<TestTableDifferentTypesV2>().ToArray();
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.StringValue), newObjs.Select(obj => obj.StringValue), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.IntValue), newObjs.Select(obj => obj.IntValue), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.DateValue), newObjs.Select(obj => Convert.ToDateTime(obj.DateValue)), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			
			// Trigger another Migraiton
			DatabaseV2 = GetDatabaseV2;
			DatabaseV2.RegisterDataObject(typeof(TestTableDifferentTypesV1));
			
			var newerObjs = DatabaseV2.SelectAllObjects<TestTableDifferentTypesV1>().ToArray();
			
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.StringValue), newerObjs.Select(obj => obj.StringValue), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.IntValue), newerObjs.Select(obj => obj.IntValue), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.DateValue), newerObjs.Select(obj => obj.DateValue), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
		}
	}
}
