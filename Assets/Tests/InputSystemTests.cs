using InputEnum;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTests : InputTestFixture
{
    private Keyboard keyboard;
    private Mouse mouse;
    private InputManager manager;
    private InputActionAsset asset;

    private bool attackCalled;
    private bool inventoryCalled;
    private Vector2 moveValue;

    public override void Setup()
    {
        base.Setup();

        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();

        asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/20_InputActions/InputActions.inputactions");
        Assert.IsNotNull(asset);

        var go = new GameObject("InputManager");
        manager = go.AddComponent<InputManager>();
        manager.Initialize(asset);

        BindActions();

        attackCalled = false;
        inventoryCalled = false;
        moveValue = Vector2.zero;
    }

    private void BindActions()
    {
        // Bind
        manager.BindInput(ActionMaps.Gameplay, Actions.Attack, OnAttack);
        manager.BindInput(ActionMaps.Gameplay, Actions.Move, OnMove);
        manager.BindInput(ActionMaps.UI, Actions.Inventory, OnInventory);
    }

    #region 인풋 바인드 테스트용
    private void OnAttack(InputAction.CallbackContext context)
    {
        attackCalled = true;
        Debug.Log("[LOG] Attack triggered");
    }

    private void OnInventory(InputAction.CallbackContext context)
    {
        inventoryCalled = true;
        Debug.Log("[LOG] Inventory triggered");
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveValue = context.ReadValue<Vector2>();
        Debug.Log($"[LOG] Move value: {moveValue}");
    }
    #endregion

    public override void TearDown()
    {
        Object.DestroyImmediate(manager.gameObject);
        base.TearDown();
    }

    [Test]
    public void 바인딩_확인_GamePlay_Attack_좌클릭()
    {
        manager.SetLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsTrue(attackCalled, "좌클릭 바인딩이 동작하지 않음");
    }

    [Test]
    public void 입력_확인_GamePlay_Move()
    {
        manager.SetLayer(ActionMaps.Gameplay);

        Press(keyboard.wKey);
        InputSystem.Update();
        Release(keyboard.wKey);
        InputSystem.Update();

        Assert.AreEqual(Vector2.up, moveValue);
    }

    [Test]
    public void ActionMap_Enum_매칭_검증()
    {
        foreach (ActionMaps map in System.Enum.GetValues(typeof(ActionMaps)))
        {
            if (map == ActionMaps.None) continue;

            Assert.DoesNotThrow(() =>
            {
                manager.SetLayer(map);
            }, $"ActionMap '{map}'이 존재하지 않음");
        }
    }

    [Test]
    public void 레이어_전체_입력_잠금()
    {
        manager.SetLayer(ActionMaps.None);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled);
    }

    [Test]
    public void 레이어_추가_Gameplay()
    {
        manager.SetLayer(ActionMaps.None);
        manager.AddLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsTrue(attackCalled);
    }

    [Test]
    public void 레이어_제거_Gameplay()
    {
        manager.SetLayer(ActionMaps.Gameplay);
        manager.RemoveLayer(ActionMaps.Gameplay);

        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled);
    }

    [Test]
    public void 입력_리바인딩_Attack()
    {
        manager.SetLayer(ActionMaps.Gameplay);

        string previousBinding = Mouse.current.leftButton.path;
        string newBinding = Keyboard.current.fKey.path;

        manager.ApplyBindingOverride(
            ActionMaps.Gameplay,
            Actions.Attack,
            newBinding);

        // 기존 좌클릭은 동작하면 안됨
        Press(mouse.leftButton);
        InputSystem.Update();

        Assert.IsFalse(attackCalled, "기존 바인딩 동작함");

        // 새 바인딩 F는 동작해야 함
        Press(keyboard.fKey);
        InputSystem.Update();

        Assert.IsTrue(attackCalled, "새 바인딩 동작 안함");

        // 테스트 후 원래 바인딩으로 복구
        asset.RemoveAllBindingOverrides();
    }

    [Test]
    public void 키_바인딩_Save_Load()
    {
        manager.SetLayer(ActionMaps.Gameplay);

        string newPath = keyboard.fKey.path;

        // 1. 리바인딩
        manager.ApplyBindingOverride(ActionMaps.Gameplay, Actions.Attack, newPath);

        // 2. JSON 추출
        string savedJson = manager.ExportBindingJson();
        Assert.IsFalse(string.IsNullOrEmpty(savedJson), "저장 안됨");

        // 3. 완전 재초기화
        manager.Initialize(asset);

        // 4. JSON 복원
        manager.ImportBindingJson(savedJson);
        manager.SetLayer(ActionMaps.Gameplay);

        bool triggered = false;
        manager.BindInput(ActionMaps.Gameplay, Actions.Attack, _ => triggered = true);

        // 기존 키는 동작하면 안됨
        Press(mouse.leftButton);
        InputSystem.Update();
        Assert.IsFalse(triggered);

        // F키는 동작해야 함
        Press(keyboard.fKey);
        InputSystem.Update();
        Assert.IsTrue(triggered);

        asset.RemoveAllBindingOverrides();
    }
}