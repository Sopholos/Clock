using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Clock.Helpers
{
	[ContentProperty("Expression")]
	public class Calculation : MarkupExtension
	{
		public IExpression Expression { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Expression == null) throw new Exception("Expression cannot be null.");

			return Expression.CalculateValue();
		}
	}

	[TypeConverter(typeof(ExpressionConverter))]
	public interface IExpression
	{
		double CalculateValue();
	}

	public abstract class BinaryOperation : IExpression
	{
		public IExpression Operand1 { get; set; }
		public IExpression Operand2 { get; set; }

		public double CalculateValue()
		{
			if (Operand1 == null) throw new Exception("Operand1 cannot be null.");
			if (Operand2 == null) throw new Exception("Operand2 cannot be null.");

			return CalculateBinaryOperation();
		}

		protected abstract double CalculateBinaryOperation();
	}
	public class Sum : BinaryOperation
	{
		protected override double CalculateBinaryOperation()
		{
			return Operand1.CalculateValue() + Operand2.CalculateValue();
		}
	}
	public class Substract : BinaryOperation
	{
		protected override double CalculateBinaryOperation()
		{
			return Operand1.CalculateValue() - Operand2.CalculateValue();
		}
	}
	public class Multiply : BinaryOperation
	{
		protected override double CalculateBinaryOperation()
		{
			return Operand1.CalculateValue() * Operand2.CalculateValue();
		}
	}

	public class Division : BinaryOperation
	{
		protected override double CalculateBinaryOperation()
		{
			return Operand1.CalculateValue() / Operand2.CalculateValue();
		}
	}

	public class Value : MarkupExtension, IExpression
	{
		public double? Double { get; set; }

		public Value() { }
		public Value(double @double)
			: this()
		{
			this.Double = @double;
		}

		public double CalculateValue()
		{
			if (Double == null) throw new Exception("Double");

			return Double.Value;
		}

		// Allows easy object instantiation in XAML attributes. (Result of StaticResource is not piped through ExpressionConverter.)
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}

	public class ExpressionConverter : DoubleConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			var doubleValue = (double)base.ConvertFrom(context, culture, value);
			return (IExpression)new Value(doubleValue);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			var val = (Value)value;
			return base.ConvertTo(context, culture, val.CalculateValue(), destinationType);
		}
	}
}
