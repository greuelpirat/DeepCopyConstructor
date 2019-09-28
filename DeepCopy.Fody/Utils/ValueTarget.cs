using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DeepCopy.Fody.Utils
{
    public class ValueTarget : IDisposable
    {
        public static ValueTarget New()
        {
            return new ValueTarget();
        }

        private PropertyDefinition _property;
        private VariableDefinition _variable;
        private MethodReference _call;
        private MethodReference _callvirt;
        private bool _loaded;

        private ICollection<Instruction> _instructions;
        private Instruction _next;

        private readonly IList<OpCode> _added = new List<OpCode>();

        private ValueTarget() { }

        public ValueTarget Property(PropertyDefinition property)
        {
            _property = property;
            return this;
        }

        public ValueTarget Variable(VariableDefinition variable)
        {
            _variable = variable;
            return this;
        }

        public ValueTarget Call(MethodReference method)
        {
            _call = method;
            return this;
        }

        public ValueTarget Callvirt(MethodReference method)
        {
            _callvirt = method;
            return this;
        }

        public ValueTarget Add(OpCode code)
        {
            _added.Add(code);
            return this;
        }

        public ValueTarget Loaded()
        {
            _loaded = true;
            return this;
        }

        public IEnumerable<Instruction> Build(VariableDefinition variable) => Build(ValueSource.New().Variable(variable));

        public IEnumerable<Instruction> Build(ValueSource source)
        {
            var instructions = new List<Instruction>();
            using (Build(instructions, out _))
                instructions.AddRange(source);
            return instructions;
        }

        public IDisposable Build(ICollection<Instruction> instructions, out Instruction next)
        {
            _instructions = instructions;
            if (_loaded)
                _loaded = false;
            else if (_variable != null)
                instructions.Add(Instruction.Create(_variable.VariableType.IsPrimitive || _property == null ? OpCodes.Ldloc : OpCodes.Ldloca, _variable));
            else
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            _next = Instruction.Create(OpCodes.Nop);
            next = _next;
            return this;
        }

        public void Dispose()
        {
            if (_instructions == null)
                throw new InvalidOperationException();

            _instructions.Add(_next);

            if (_property != null)
                _instructions.Add(_property.MakeSet());
            else if (_call != null)
                _instructions.Add(Instruction.Create(OpCodes.Call, _call));
            else if (_callvirt != null)
                _instructions.Add(Instruction.Create(OpCodes.Callvirt, _callvirt));

            foreach (var code in _added)
                _instructions.Add(Instruction.Create(code));

            _instructions = null;
        }
    }
}