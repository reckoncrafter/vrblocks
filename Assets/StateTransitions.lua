StateTransitions = {
  [Pink] = {
    transitions = {
      'Blue',
    },
  },
  [Blue] = {
    transitions = {
      'Yellow',
    },
  },
  [Yellow] = {
    transitions = {
      'Pink',
    },
  },
}

function onTransition(stateName)
print("Transition: " .. stateName)
UnityFunction(stateName)
end