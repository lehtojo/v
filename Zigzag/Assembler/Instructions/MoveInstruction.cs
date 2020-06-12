using System;
using System.Reflection.Metadata;

public enum MoveType
{
	/// <summary>
	/// The source value is loaded to the destination attaching the source value to the destination and leaving the source untouched
	/// </summary>
	COPY,
	/// <summary>
	/// The source value is loaded to destination attaching the destination value to the destination
	/// </summary>
	LOAD,
	/// <summary>
	/// The source value is loaded to the destination attaching the source value to the destination and updating the source to be equal to the destination
	/// </summary>
	RELOCATE
}

public class MoveInstruction : DualParameterInstruction
{
	public const string MOVE_INSTRUCTION = "mov";
	public const string UNSIGNED_CONVERSION = "movzx";
	public const string SIGNED_CONVERSION = "movsx";
	public const string SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE = "movsxd";
	
	public const string SINGLE_PRECISION_MOVE = "movss";
	public const string DOUBLE_PRECISION_MOVE = "movsd";

	public const string CONVERT_SINGLE_PRECISION_TO_INTEGER = "cvtss2si";
	public const string CONVERT_DOUBLE_PRECISION_TO_INTEGER = "cvtsd2si";

	public const string CONVERT_INTEGER_TO_SINGLE_PRECISION = "cvtsi2ss";
	public const string CONVERT_INTEGER_TO_DOUBLE_PRECISION = "cvtsi2sd";

	public const string CLEAR_INSTRUCTION = "xor";

	private MoveType _Type = MoveType.COPY;
	public new MoveType Type 
	{
		get => _Type;

		set 
		{
			_Type = value;
			UpdateResultFormat();
		}
	}

	public bool IsSafe { get; set; } = false;
	public bool IsRedundant => First.Value.Equals(Second.Value);

	public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format) 
	{
		IsFutureUsageAnalyzed = false;
	}

	private void UpdateResultFormat()
	{
		Result.Format = (Type == MoveType.LOAD) ? First.Format : Second.Format;
	}

	private bool IsDecimalConversionNeeded()
	{
		return First.Format != Second.Format;
	}

	private void OnBuildDecimalConversion(int flags_first, int flags_second)
	{
		var is_destination_media_register = First.IsMediaRegister;
		var is_destination_register = First.IsRegister;
		var is_destination_memory_address = First.IsMemoryAddress;
		var is_source_constant = Second.IsConstant;
      string? instruction;
		
      if (is_destination_media_register)
		{
			// Destination: integer
			// Source: decimal
			//
			// Examples:
			// 
			// 10 => xmm0
			// movsd xmm0, qword [C0]
			//
			// C0 dq 10.0
			//
			// rax => xmm1
			// cvtsi2sd xmm1, rax
			//
			// [rsp+16] => xmm2
			// cvtsi2sd xmm1, qword [rsp+16]

			if (is_source_constant)
			{
				// Ensure the source value is in decimal format
				Second.Value.To<ConstantHandle>().Convert(global::Format.DECIMAL);
				Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());

				instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

				// Examples:
				// movsd xmm0, qword [C1] (C1: 10.0)

				Build(
					instruction,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEMORY
					)
				);
			}
			else
			{
				/*if (Type == MoveType.RELOCATE)
				{
					throw new ApplicationException("Warning: Relocating and converting type at the same time");
				}*/

				instruction = Assembler.IsTargetX86 ? CONVERT_INTEGER_TO_SINGLE_PRECISION : CONVERT_INTEGER_TO_DOUBLE_PRECISION;

				Build(
					instruction,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.MEDIA_REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.REGISTER,
						HandleType.MEMORY
					)
				);
			}
		}
		else if (is_destination_register)
		{
			// Destination: integer
			// Source: decimal
			//
			// Examples:
			// 
			// 3.14159 => rax
			// mov rax, 3
			//
			// xmm0 => rax
			// cvtsd2si rax, xmm0
			//
			// [rsp+16] => rax
			// cvtsd2si rax, qword [rsp+16]

			if (is_source_constant)
			{
				// Ensure the source value is in integer format
				Second.Value.To<ConstantHandle>().Convert(First.Format);

				instruction = MOVE_INSTRUCTION;

				Build(
					instruction,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.CONSTANT
					)
				);
			}
			else
			{
				instruction = Assembler.IsTargetX86 ? CONVERT_SINGLE_PRECISION_TO_INTEGER : CONVERT_DOUBLE_PRECISION_TO_INTEGER;

				Build(
					instruction,
					new InstructionParameter(
						First,
						flags_first,
						HandleType.REGISTER
					),
					new InstructionParameter(
						Second,
						flags_second,
						HandleType.MEDIA_REGISTER,
						HandleType.MEMORY
					)
				);
			}
		}
		else if (is_destination_memory_address)
		{
			if (!First.Format.IsDecimal())
			{
				// Destination: integer
				// Source: decimal
				//
				// Examples:
				//
				// xmm0 => [rsp+8]:
				// cvtsd2si rax, xmm0
				// mov qword [rsp+8], rax
				//
				// [rsp+32] (decimal) => [rsp+64] (integer)
				// cvtsd2si rax, qword [rsp+32]
				// mov qword [rsp+64], rax
				//
				// 3.141 => [rsp+16]:
				// mov qword [rsp+16], 3

				if (is_source_constant)
				{
					// Convert the decimal value to the destination's integer format
					Second.Value.To<ConstantHandle>().Convert(First.Format);

					Build(
						MOVE_INSTRUCTION,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.CONSTANT
						)
					);
				}
				else
				{
					Build(
						MOVE_INSTRUCTION,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.REGISTER
						)
					);
				}
			}
			else
			{
				// Destination: decimal
				// Source: integer
				//
				// Examples:
				//
				// rax => [rsp+8]:
				// ctvsi2sd xmm0, rax
				// movsd qword [rsp+8], xmm0
				//
				// 10 => [rsp+16]:
				// mov rax, 10.0
				// mov qword [rsp+16], rax

				if (is_source_constant)
				{
					// Convert the integer value to the destination's decimal format
					Second.Value.To<ConstantHandle>().Convert(First.Format);

					Build(
						MOVE_INSTRUCTION,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.REGISTER
						)
					);
				}
				else
				{
					instruction = Assembler.IsTargetX86 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

					Build(
						instruction,
						new InstructionParameter(
							First,
							flags_first,
							HandleType.MEMORY
						),
						new InstructionParameter(
							Second,
							flags_second,
							HandleType.MEDIA_REGISTER
						)
					);
				}
			}
		}
	}

	private void OnBuildDecimalMoves(int flags_first, int flags_second)
	{
		if (IsDecimalConversionNeeded())
		{
			OnBuildDecimalConversion(flags_first, flags_second);
		}
		else
		{
			var size = First.Size;
			var instruction = size.Bits == 32 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

			if (Second.IsConstant)
			{
				// Move the source value to the data section so that it can be loaded to a media register
				Second.Value = new ConstantDataSectionHandle(Second.Value.To<ConstantHandle>());
			}

			Build(
				instruction,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.MEDIA_REGISTER,
					HandleType.MEMORY
				)
			);
		}
	}

	public override void OnBuild()
	{
		UpdateResultFormat();

		// Move shouldn't happen if the source is the same as the destination
		if (IsRedundant) return;

		if (IsSafe && First.IsRegister)
		{
			Memory.ClearRegister(Unit, First.Value.To<RegisterHandle>().Register);
		}

		var flags_first = ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS);
		var flags_second = ParameterFlag.NONE;

		switch (Type)
		{
			case MoveType.COPY:
			{
				// Source value must be attached to the destination
				flags_second |= ParameterFlag.ATTACH_TO_DESTINATION;
				break;
			}

			case MoveType.LOAD:
			{
				// Destination value must be attached to the destination
				flags_first |= ParameterFlag.ATTACH_TO_DESTINATION;
				break;
			}

			case MoveType.RELOCATE:
			{
				// Source value must be attached and relocated to destination
				flags_second |= ParameterFlag.ATTACH_TO_DESTINATION | ParameterFlag.RELOCATE_TO_DESTINATION;
				break;
			}
		}

		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			// Decimals have their own handler
			OnBuildDecimalMoves(flags_first, flags_second);
			return;
		}

		if (First.IsRegister && Second.IsConstant && Second.Value.To<ConstantHandle>().Value.Equals(0L))
		{
			// Example: xor rax, rax
			Build(
				CLEAR_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second| ParameterFlag.HIDDEN,
					HandleType.CONSTANT
				)
			);
		}
		else if (First.IsMemoryAddress)
		{
			// Examples:
			// mov [rsp+8], 314159
			// mov [rdi], -1
			Build(
				MOVE_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.CONSTANT,
					HandleType.REGISTER
				)
			);
		}
		else
		{
			// Examples:
			// mov rax, 7
			// mov rbx, rax
			// mov rcx, [rsp+8]
			Build(
				MOVE_INSTRUCTION,
				new InstructionParameter(
					First,
					flags_first,
					HandleType.REGISTER,
					HandleType.MEMORY
				),
				new InstructionParameter(
					Second,
					flags_second,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}
	}

	public override void OnPostBuild()
	{
		var is_source_memory_address = Source!.Value?.Type == HandleType.MEMORY;
		var is_destination_memory_address = Destination!.Value?.Type == HandleType.MEMORY;

		if (is_source_memory_address)
		{
			if (is_destination_memory_address)
			{
				throw new ApplicationException("Both destination and source were memory handles at the same time in move instruction");
			}

			// NOTE: Now the destination parameter must be a register
			
			// Check if a conversion is needed
			if (Source!.Value!.Size != Destination!.Value!.Size)
			{
				// In 32-bit mode there's only one conversion instruction type needed
				if (Assembler.IsTargetX86)
				{
					Operation = Destination.Value.IsUnsigned ? UNSIGNED_CONVERSION : SIGNED_CONVERSION;
				}
				else if (Assembler.IsTargetX64)
				{
					if (Destination.Value.Size.Bits != 64)
					{
						throw new ApplicationException("Destination register should be 64-bit in 64-bit mode always");
					}

					if (Source.Value.Size.Bits == 32)
					{
						if (Destination.Value.IsUnsigned)
						{
							// In 64-bit mode if you move data from 32-bit register to another 32-bit register it zeroes out the high half of the destination 64-bit register
							Destination.Value.Format = global::Format.UINT32;
						}
						else
						{
							Operation = SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE;
						}
					}
					else
					{
						Operation = Destination.Value!.IsUnsigned ? UNSIGNED_CONVERSION : SIGNED_CONVERSION;
					}
				}
				else
				{
					throw new ApplicationException("Conversion needed in move instruction but current bitmode is unsupported");
				}
			}
		}
		else if (is_destination_memory_address)
		{
			Source.Value!.Format = Destination.Value!.Format;
		}
	}

	public override Result GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.MOVE;
	}
}