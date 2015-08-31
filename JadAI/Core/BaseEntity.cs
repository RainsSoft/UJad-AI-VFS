#region Using Directives

using System;

#endregion

namespace JadEngine.AI.Core
{
	/// <summary>
	/// This class is the base class any entity in the game must inherit from. It gives an ID to the 
	/// entity so it can register in the EntityManager to allow access from any point of the program
	/// </summary>
	[Serializable]
	public abstract class BaseEntity
	{
		#region Constants

		/// <summary>
		/// Indicates that an entity is not registered and it doesn´t have an ID assigned
		/// </summary>
		public const int EntityNotRegistered = -1;

		#endregion

		#region Fields

		/// <summary>
		/// The unique ID of the entity. It´s used to access the entity when needed.
		/// </summary>
		protected internal int id = BaseEntity.EntityNotRegistered;

		/// <summary>
		/// This value indicates in the entity should be removed from the manager or not
		/// </summary>
		protected internal bool remove;

		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor. Registers the entity in the EntityManager
		/// </summary>
		public BaseEntity()
		{
			remove = false;
			EntityManager.Instance.AddEntity(this);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the entity ID. Only the EntityManager can set the ID for the entity.
		/// </summary>
		public int ID
		{
			get
			{
				return id;
			}

			protected internal set
			{
				id = value;
			}
		}

		/// <summary>
		/// Gets or sets the value indicating in the entity should be removed from the manager
		/// </summary>
		public bool Remove
		{
			get { return remove; }
			set { remove = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Updates the entity
		/// </summary>
		/// <param name="elapsedTime">The time that passed since the last update</param>
		public abstract void Update(float elapsedTime);

		/// <summary>
		/// Method executed when the entity is destroyed
		/// </summary>
		/// <remarks>This method should be used for clean up</remarks>
		protected internal virtual void Destroy()
		{}

		#endregion
	}
}
