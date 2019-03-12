import { BaseEntity } from "app/classes/base-entity";

export class Holiday extends BaseEntity {
    name: string;
    start: Date;
    end: Date;
}