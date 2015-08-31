#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace JadEngine.AI.Core
{
	/// <summary>
	/// This class keeps a list of all entities in the game. It uses a thread-safe
	/// singleton implementation
	/// </summary>
	public sealed class EntityManager
	{
		#region Fields

		/// <summary>
		/// Singleton variable. Uses lazy instantiation
		/// </summary>
		private static readonly EntityManager manager = new EntityManager();

		/// <summary>
		/// Number of registered entities
		/// </summary>
		private int idCount;

		/// <summary>
		/// Dictionary where we are going to store all the entities
		/// </summary>
		private Dictionary<int, BaseEntity> entities;

		#endregion

		#region Constructors

		/// <summary>
		/// Static constructor needed to be thread safe
		/// </summary>
		static EntityManager() { }

		/// <summary>
		/// Default constructor
		/// </summary>
		private EntityManager()
		{
			idCount = 0;
			entities = new Dictionary<int, BaseEntity>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the EntityManager instance
		/// </summary>
		public static EntityManager Instance
		{
			get { return manager; }
		}

		/// <summary>
		/// Gets an entity based on its ID
		/// </summary>
		/// <param name="id">The ID of the entity we want to recover</param>
		/// <returns></returns>
		public BaseEntity this[int id]
		{
			get
			{
				return entities[id];
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Registers an entity in the EntityManager and assigns it an unique ID
		/// </summary>
		/// <param name="entity">The entity we want to register</param>
		/// <returns>The ID of the new entity</returns>
		public int AddEntity(BaseEntity entity)
		{
			//If the entity was already registered return
			if (entity.ID != BaseEntity.EntityNotRegistered)
				if (entities[entity.ID] != null && entities[entity.ID] == entity)
					return entity.ID;

			//If not, we assign it an ID and register it (if the entity had an ID then overwrite it)
			entity.ID = ++idCount;
			entities.Add(idCount, entity);

			return idCount;
		}

		/// <summary>
		/// Removes an entity from the EntityManager and unassigns its ID
		/// </summary>
		/// <param name="entity">The entitiy we want to remove</param>
		public void RemoveEntity(BaseEntity entity)
		{
			if (entity.ID == BaseEntity.EntityNotRegistered)
				return;

			entities.Remove(entity.ID);
			entity.ID = BaseEntity.EntityNotRegistered;
			entity.Destroy();
		}

		/// <summary>
		/// Resets the entity manager. All info in the manger will be lost
		/// </summary>
		public void Clear()
		{
			idCount = 0;

			foreach (BaseEntity entity in entities.Values)
			{
				entity.ID = BaseEntity.EntityNotRegistered;
				entity.Destroy();
			}

			entities = new Dictionary<int, BaseEntity>();
		}

		/// <summary>
		/// Returns the list of entities in the manager of type T
		/// </summary>
		/// <typeparam name="T">The type to cast the list of entities</typeparam>
		/// <returns>The list of entities of the manager of type T</returns>
		/// <remarks>This method has to cast all the entities, so it´s quite slow</remarks>
		public List<T> GetEntitiesList<T>() where T : BaseEntity
		{
			List<T> entitiesList;
			T trial;

			entitiesList = new List<T>();

			foreach (BaseEntity entity in entities.Values)
			{
				trial = entity as T;
				if (trial != null)
					entitiesList.Add(trial);
			}

			return entitiesList;
		}

		/// <summary>
		/// Updates all entities registered in the entity manager
		/// </summary>
		/// <remarks>The entities marked for removal will be destroyed</remarks>
		public void Update()
		{
			List<BaseEntity> deletedEntities;

			deletedEntities = new List<BaseEntity>();
			foreach (BaseEntity entity in entities.Values)
			{
				//If the entity should be removed, don´t update it and mark it for removal
				if (entity.Remove == true)
					deletedEntities.Add(entity);

				else
					entity.Update(UnityEngine.Time.deltaTime);
			}

			//Destroy the entities marked
			for (int i = 0; i < deletedEntities.Count; i++)
			{
				deletedEntities[i].Destroy();
				entities.Remove(deletedEntities[i].ID);
			}
		}

		#endregion
	}
}
