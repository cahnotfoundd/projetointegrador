using UnityEngine;
using UnityEngine.EventSystems;

//animar botão, forçando a obedecer os comandos.
public class AnimationButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    //criando variável que guarda o animator, para mandar a ele depois os comandos da animação.
    private Animator animator;
    //variável para determinar se o mouse está sobre o botão
    private bool isMouseOver = false;
    //variável para determinar se o mouse está clicando no botão
    private bool isMouseClicking = false;

    // criando uma função para pegar o componente animator, que vai ser chamado no Start.
    private void Awake()
    {
        //pegando o componente animator do botão
        animator = GetComponent<Animator>();  
    }

    // função para ativar cada estado da animação, quando o comando estiver em execução.
    private void ActivateState(string stateName)
    {
        //resetando os triggers para que não haja conflito entre eles, e o botão não fique preso em um estado.
        animator.ResetTrigger("Normal");
        animator.ResetTrigger("Highlighted");
        animator.ResetTrigger("Pressed");
        //ativando o trigger do estado que foi chamado.
        animator.SetTrigger(stateName);
    }

    //ativando hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        if (!isMouseClicking)
        {
            ActivateState("Highlighted");
        }
    }

    //desativando hover
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        if (!isMouseClicking)
        {
            ActivateState("Normal");
        }
    }

    //ativando clique
    public void OnPointerDown(PointerEventData eventData)
    {
        isMouseClicking = true;
        ActivateState("Pressed");
    }

    //condição para quando o mouse estiver sobre o botão, ele voltar para hover e quando sair, voltar para normal
    public void OnPointerUp(PointerEventData eventData)
    {
        isMouseClicking = false;

        if (isMouseOver)
        {
            ActivateState("Highlighted");
        }
        else
        {
            ActivateState("Normal");
        }
    }
}


