    GameObject rootGo = new GameObject(name);
    
    GameObject visualGo = new GameObject("Visuals");
    visualGo.transform.SetParent(rootGo.transform, false);
    var sr = visualGo.AddComponent<SpriteRenderer>();
    // ... setup sprite
    
    GameObject chargeGo = new GameObject("ChargeVisual");
    chargeGo.transform.SetParent(rootGo.transform, false);
