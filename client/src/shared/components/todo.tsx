import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Check, X } from "lucide-react";
import { Input } from "@/shared/components/ui/input";
import { Button } from "@/shared/components/ui/button";
import { useState } from "react";

type Todo = {
    id: number;
    text: string;
    completed: boolean;
}


export default function Todo(){
    const [todos, setTodos] = useState<Todo[]>([
    {id: 1, text: "Learn TypeScript", completed: false},
    {id: 2, text: "Learn Build a Next.js app", completed: true},
    {id: 3, text: "Write tests", completed: false}]);

    const addTodo = () => {
   const newTodo: Todo = {
    id: todos.length + 1,
    text: input,
    completed: false
   };

   setTodos(prevTodos => [...prevTodos, newTodo]);
};
 const[input, setInput] = useState("");

    return (
        <div className="flex flex-col gap-4 p-4">
            <div className="flex gap-2">
                <Input placeholder="Название задачи" className="flex-1" 
                value={input}
                onChange={(event) => setInput(event.target.value)}/>
                <Button onClick={addTodo}>Добавить</Button>
            </div>
            {todos.map((todo) => (
                <Card key={todo.id}>
                    <CardHeader>
                        <CardTitle className="flex items-center justify-between">
                            <span>{todo.text}</span>
                            {todo.completed ? (
                                <Check className="h-5 w-5 text-green-600" />
                            ) : (
                                <X className="h-5 w-5 text-red-600" />
                            )}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className={`text-sm ${todo.completed ? "text-green-600" : "text-zinc-500"}`}>
                            Status: {todo.completed ? "Completed" : "Pending"}
                        </p>
                    </CardContent>
                </Card>
            ))}
        </div>
    );
}
