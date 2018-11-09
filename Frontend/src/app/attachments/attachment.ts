import {BaseEntity} from '../classes/base-entity';

export class Attachment extends BaseEntity {
  public name: string;
  public fileType: string;
  public googleId: string;
  public downloadUrl: string;
  public attachedToId: string;
}
