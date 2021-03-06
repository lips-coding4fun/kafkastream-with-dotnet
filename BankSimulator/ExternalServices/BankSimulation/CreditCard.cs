// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.11.0.0
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace BankSimulation
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Avro;
	using Avro.Specific;
	
	public partial class CreditCard : ISpecificRecord
	{
		public static Schema _SCHEMA = Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"CreditCard\",\"namespace\":\"BankSimulation\",\"fields\":[{\"nam" +
				"e\":\"cardNumber\",\"type\":\"string\"},{\"name\":\"code\",\"type\":\"string\"},{\"name\":\"owner\"" +
				",\"type\":\"string\"},{\"name\":\"initialMoney\",\"type\":\"float\"}]}");
		private string _cardNumber;
		private string _code;
		private string _owner;
		private float _initialMoney;
		public virtual Schema Schema
		{
			get
			{
				return CreditCard._SCHEMA;
			}
		}
		public string cardNumber
		{
			get
			{
				return this._cardNumber;
			}
			set
			{
				this._cardNumber = value;
			}
		}
		public string code
		{
			get
			{
				return this._code;
			}
			set
			{
				this._code = value;
			}
		}
		public string owner
		{
			get
			{
				return this._owner;
			}
			set
			{
				this._owner = value;
			}
		}
		public float initialMoney
		{
			get
			{
				return this._initialMoney;
			}
			set
			{
				this._initialMoney = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.cardNumber;
			case 1: return this.code;
			case 2: return this.owner;
			case 3: return this.initialMoney;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.cardNumber = (System.String)fieldValue; break;
			case 1: this.code = (System.String)fieldValue; break;
			case 2: this.owner = (System.String)fieldValue; break;
			case 3: this.initialMoney = (System.Single)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
