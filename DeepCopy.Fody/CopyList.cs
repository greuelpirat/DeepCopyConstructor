using System.Collections.Generic;
using System.Linq;
using DeepCopy.Fody.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DeepCopy.Fody
{
    public partial class ModuleWeaver
    {
        private IEnumerable<Instruction> CopyList(PropertyDefinition property)
        {
            if (IsCopyConstructorAvailable(property.PropertyType, out _))
                return CopyItem(property);

            return CopyList(property.PropertyType, property);
        }

        private IEnumerable<Instruction> CopyList(TypeReference type, PropertyDefinition property)
        {
            var loopStart = Instruction.Create(OpCodes.Nop);
            var index = NewVariable(TypeSystem.Int32Definition);
            var loopCheck = NewVariable(TypeSystem.BooleanDefinition);
            var conditionStart = Instruction.Create(OpCodes.Ldloc, index);

            var constructor = ConstructorOfSupportedType(type, typeof(IList<>), typeof(List<>), out var typesOfArguments);

            var typeOfList = type.Resolve();
            var typeOfArgument = typesOfArguments.Single();

            var list = new List<Instruction>();
            if (property != null)
            {
                list.Add(Instruction.Create(OpCodes.Ldarg_0));
                list.Add(Instruction.Create(OpCodes.Newobj, constructor));
                list.Add(property.MakeSet());
            }

            list.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            list.Add(Instruction.Create(OpCodes.Stloc, index));
            list.Add(Instruction.Create(OpCodes.Br_S, conditionStart));
            list.Add(loopStart);

            list.AddRange(CopyListItem(property, typeOfList, typeOfArgument, index));

            // increment index
            list.Add(Instruction.Create(OpCodes.Ldloc, index));
            list.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            list.Add(Instruction.Create(OpCodes.Add));
            list.Add(Instruction.Create(OpCodes.Stloc, index));

            // condition
            list.Add(conditionStart);
            list.Add(Instruction.Create(OpCodes.Ldarg_1));
            if (property != null)
                list.Add(Instruction.Create(OpCodes.Callvirt, property.GetMethod));
            list.Add(Instruction.Create(OpCodes.Callvirt, ImportMethod(typeOfList, "get_Count", typeOfArgument)));
            list.Add(Instruction.Create(OpCodes.Clt));
            list.Add(Instruction.Create(OpCodes.Stloc, loopCheck));

            // loop end
            list.Add(Instruction.Create(OpCodes.Ldloc, loopCheck));
            list.Add(Instruction.Create(OpCodes.Brtrue_S, loopStart));

            return list;
        }

        private IEnumerable<Instruction> CopyListItem(PropertyDefinition property, TypeReference listType, TypeReference argumentType, VariableDefinition index)
        {
            var list = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldarg_0)
            };
            if (property != null)
                list.Add(Instruction.Create(OpCodes.Call, property.GetMethod));

            var listItemGetter = ValueSource.New().Property(property).Index(index).Method(ImportMethod(listType, "get_Item", argumentType));

            list.AddRange(CopyValue(argumentType, listItemGetter));
            list.Add(Instruction.Create(OpCodes.Callvirt, ImportMethod(listType, nameof(IList<object>.Add), argumentType)));

            return list;
        }
    }
}