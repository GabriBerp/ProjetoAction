using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] private int capacity = 10;

    // Stack = LIFO: o último a entrar é o primeiro a sair
    private readonly Stack<IPickable> items = new Stack<IPickable>();

    public int Count => items.Count;
    public int Capacity => capacity;
    public bool IsFull => items.Count >= capacity;

    // Topo da pilha primeiro (LIFO). Usado pela interface espacial da sacola.
    public IEnumerable<IPickable> Items => items;

    // Disparado a cada mudanca no conteudo (guardar, retirar, descarte de item destruido).
    public event Action Changed;

    /// <summary>Guarda o item na bolsa: solta da mão e desativa o objeto.</summary>
    public bool TryStore(IPickable item)
    {
        if (item == null || IsFull) return false;

        var component = item as Component;
        if (component == null) return false;

        // Solta da mão sem força, para o item não estar mais "segurado" (IsHeld = false)
        item.Drop(Vector3.zero);

        component.gameObject.SetActive(false);
        items.Push(item);
        Changed?.Invoke();
        return true;
    }

    /// <summary>Tira o último item guardado, reativa o objeto e devolve ele.</summary>
    public bool TryRetrieve(out IPickable item)
    {
        item = null;
        bool changed = false;

        while (items.Count > 0)
        {
            var candidate = items.Pop();
            changed = true;
            var component = candidate as Component;

            // Pula itens que foram destruídos enquanto estavam na bolsa
            if (component == null) continue;

            component.gameObject.SetActive(true);
            item = candidate;
            break;
        }

        if (changed) Changed?.Invoke();
        return item != null;
    }
}