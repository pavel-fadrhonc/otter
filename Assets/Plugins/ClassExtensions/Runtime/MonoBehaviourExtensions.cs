//  <copyright file="MonoBehaviourExtensions.cs" company="Oakus">
//  Copyright (c) 2014 All Right Reserved, http://oakus.cz//
// 
//  </copyright>
//  <author>Pavel</author>
//  <email>pavel.fadrhonc@xinity.com</email>
//  <date>2/10/2014 4:24:30 PM</date>
//  <summary></summary>

using System;
using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static Coroutine StopCoroutineSafely(this MonoBehaviour mb, Coroutine c)
    {
        if (c != null)
        {
            mb.StopCoroutine(c);
        }
        return null;
    }    
    
    /// <summary>
    /// Invokes the method methodtask in time seconds.
    /// </summary>
    public static void Invoke(this MonoBehaviour mono, Action task, float time)
    {
        mono.Invoke(task.Method.Name, time);
    }

    /// <summary>
    /// Invokes the method task in time seconds.
    /// After the first invocation repeats calling that function every repeatRate seconds.
    /// </summary>
    public static void InvokeRepeating(this MonoBehaviour mono, Action task, float time, float repeatRate)
    {
        mono.InvokeRepeating(task.Method.Name, time, repeatRate);
    }

    /// <summary>
    /// Cancels all Invoke calls task on this behaviour.
    /// </summary>
    public static void CancelInvoke(this MonoBehaviour mono, Action task)
    {
        mono.CancelInvoke(task.Method.Name);
    }

    /// <summary>
    /// Is any invoke of task pending?
    /// </summary>
    public static bool IsInvoking(this MonoBehaviour mono, Action task)
    {
        return mono.IsInvoking(task.Method.Name);
    }

    /// <summary>
    /// Get first component of type T traversing up the hierachy
    /// </summary>
    public static T GetComponentInAncestor<T>(this MonoBehaviour mono) where T : Component
    {
        Transform currentTransform = mono.transform;

        while (currentTransform != null)
        {
            T component = currentTransform.gameObject.GetComponent<T>();
            if (component != null)
                return component;

            currentTransform = currentTransform.parent;
        }

        return null;
    }

}