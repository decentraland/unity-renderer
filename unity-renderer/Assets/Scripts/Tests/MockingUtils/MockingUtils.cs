public interface IDummyEventSubscriber { void React(); }
public interface IDummyEventSubscriber<T1> { void React(T1 p1); }
public interface IDummyEventSubscriber<T1, T2> { void React(T1 p1, T2 p2); }
public interface IDummyEventSubscriber<T1, T2, T3> { void React(T1 p1, T2 p2, T3 p3); }
public interface IDummyEventSubscriber<T1, T2, T3, T4> { void React(T1 p1, T2 p2, T3 p3, T4 p4); }
public interface IDummyEventSubscriber<T1, T2, T3, T4, T5> { void React(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5); }